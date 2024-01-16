using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using EscapeTheMaze.Entities;
using static System.ConsoleColor;
using static EscapeTheMaze.Managers.UserManager;

namespace EscapeTheMaze.Managers
{
	public static class GameManager
	{
		// Constants
		public const int MaxRounds = 10;

		// Constants - Score/Balance modifiers
		public const int ExitPointModifier     = 30;
		public const int BonusPointModifier    = 20;

		public const int CoinModifierMin       = 15;
		public const int CoinModifierMax       = 50;

		// Constants - Spawn chances & max occurrences
		public const double BonusPointSpawnChance  = 45;  // Percent
		public const double CoinSpawnChance        = 40;
		public const double FirstAidKitSpawnChance = 22.5;
		public const double HourglassSpawnChance   = 25;

		public const int MaxBonusPoints  = 3;
		public const int MaxCoins        = 2;
		public const int MaxFirstAidKits = 2;
		public const int MaxHourglasses  = 1;

		// Constants - Walls
		public const int WallTopOffset = 2;
		private const int WallDensityMin = 520;
		private const int WallDensityMax = 600;

		// Utilities
		private static readonly GameRandom random = new(Environment.TickCount + Guid.NewGuid().GetHashCode());

		private static readonly Stopwatch gameStopwatch = new();
		private static readonly Stopwatch roundStopwatch = new();
		private static Timer roundTimer;

		// Round variables
		private static int currentRound;
		private static int currentHearts;
		private static int currentArmors;
		private static int currentScore;
		private static int currentBalance;
		private static int currentTimeLeft;
		private static RoundState roundState;

		private static Position initialPlayerPosition;  // Used to respawn the player whenever necessary

		// Entity-related variables
		private static readonly ConsoleColor[] wallColors = {
			DarkGreen, Cyan, DarkRed, DarkMagenta, DarkYellow, Gray, DarkGray, Cyan, White
		};
		private static ConsoleColor wallColor;

		private static Player player;
		private static ExitPoint exitPoint;

		private static readonly List<Wall> walls               = new();
		private static readonly List<BonusPoint> bonusPoints   = new();
		private static readonly List<Coin> coins               = new();
		private static readonly List<FirstAidKit> firstAidKits = new();
		private static readonly List<Hourglass> hourglasses    = new();

		public static void Play()
		{
			using (roundTimer = new(1000))  // Interval: 1s
			{
				roundTimer.Elapsed += RoundTimer_Elapsed;
				ResetGameVariables();

				while (++currentRound <= MaxRounds)
				{
					IOManager.StartNewPage();

					ResetRoundVariables();
					GenerateEntities();

					if (!CurrentUser.HasEverPlayed)
						DisplayGameTip();

					while (roundState == RoundState.Ongoing)
					{
						GetControlInput();
						SetTimersEnabled(true);

						HandleCollision(GetCollisionType());

						// Check for general failure conditions
						if (currentHearts <= 0 || currentTimeLeft <= 0)
							roundState = RoundState.Failure;
					}

					AudioManager.StopPlayback();

					SetTimersEnabled(false);
					DisplayRoundStats();

					if (roundState == RoundState.Failure)
						break;
				}

				DisplayGameStats();
				SavePlayerData();
			}

            roundTimer = null;

			static void ResetGameVariables()
			{
				currentRound    = 0;
				currentHearts   = UpgradeManager.MaxHearts;
				currentArmors   = CurrentUser.Armors;
				currentScore    = 0;
				currentBalance  = 0;
				currentTimeLeft = UpgradeManager.RoundDuration;

				gameStopwatch.Reset();
			}

			static void ResetRoundVariables()
			{
				roundStopwatch.Reset();

				currentTimeLeft = UpgradeManager.RoundDuration;
				roundState = RoundState.Ongoing;

				walls.Clear();
				bonusPoints.Clear();
				coins.Clear();
				firstAidKits.Clear();
				hourglasses.Clear();

				wallColor = wallColors[random.Next(wallColors.Length)];
			}

			static void GenerateEntities()
			{
				GenerateBorders();
				GenerateWalls();
				SpawnEntities();

				static void GenerateBorders()
				{
					// Horizontial borders
					for (int i = 0; i < Constants.WindowWidth; ++i)
					{
						SpawnEntityAt(EntityType.Wall, new(i, WallTopOffset));
						SpawnEntityAt(EntityType.Wall, new(i, Constants.WindowHeight - 1));
					}

					// Vertical borders
					for (int i = WallTopOffset; i < Constants.WindowHeight - 1; ++i)
					{
						SpawnEntityAt(EntityType.Wall, new(0, i));
						SpawnEntityAt(EntityType.Wall, new(Constants.WindowWidth - 1, i));
					}
				}

				static void GenerateWalls()
				{
					int numWalls = random.Next(WallDensityMin, WallDensityMax + 1);

					for (int i = 0; i < numWalls; ++i)
						SpawnEntityAt(EntityType.Wall, random.NextPosition());
				}

				static void SpawnEntities()
				{
					// Primary entities
					SpawnEntityAt(EntityType.Player, random.NextPosition(true));
					SpawnEntityAt(EntityType.ExitPoint, random.NextPositionFarFrom(player.Position, true));

					// Secondary entities
					SpawnEntityWithChance(EntityType.BonusPoint, BonusPointSpawnChance, MaxBonusPoints);
					SpawnEntityWithChance(EntityType.Coin, CoinSpawnChance, MaxCoins);
					SpawnEntityWithChance(EntityType.FirstAidKit, FirstAidKitSpawnChance, MaxFirstAidKits);
					SpawnEntityWithChance(EntityType.Hourglass, HourglassSpawnChance, MaxHourglasses);
					
					// Ensure that no walls surround the player & exit point
					ClearWallsAroundEntity(player);
					ClearWallsAroundEntity(exitPoint);

					static void SpawnEntityWithChance(EntityType entityType, double chance, int maxOccurrences)
					{
						for (int i = 0; i < maxOccurrences; ++i)
						{
							if (random.HasRandomChanceOccurred(chance))
							{
								var position = random.NextPosition();

								walls.RemoveAll(IsAtPositionPredicate(position));
								SpawnEntityAt(entityType, position);
							}
						}
					}

					static void ClearWallsAroundEntity(Entity entity)
					{
						var position = entity.Position;

						for (int i = position.Left - 1; i <= position.Left + 1; ++i)
						{
							for (int j = position.Top - 1; j <= position.Top + 1; ++j)
							{
								var newPosition = new Position(i, j);

								if (walls.Any(IsAtPosition(newPosition)))
									walls.First(IsAtPosition(newPosition)).Hide();

								walls.RemoveAll(IsAtPositionPredicate(newPosition));
							}
						}
						entity.Draw();
					}
				}

				static void SpawnEntityAt(EntityType entityType, Position position)
				{
					Entity entity;

					switch (entityType)
					{
						case EntityType.Player:
							entity = new Player(position);
							player = entity as Player;

							initialPlayerPosition = player.Position;
							break;

						case EntityType.Wall:
							AddToList(walls, wallColor);
							break;

						case EntityType.ExitPoint:
							entity = new ExitPoint(position);
							exitPoint = entity as ExitPoint;
							break;

						case EntityType.BonusPoint:
							AddToList(bonusPoints);
							break;

						case EntityType.Coin:
							AddToList(coins);
							break;

						case EntityType.FirstAidKit:
							AddToList(firstAidKits);
							break;

						case EntityType.Hourglass:
							AddToList(hourglasses);
							break;

						default:
							throw new ArgumentOutOfRangeException(nameof(entityType), "Attempted to spawn an unknown entity.");
					}
					entity.Draw();

					void AddToList<T>(List<T> entityList, ConsoleColor? entityColor = null) where T : Entity, new()
					{
						entity = new T() { Position = position };

						if (entityColor is not null)
							entity.Color = (ConsoleColor)entityColor;

						entityList.Add((T)entity);
					}
				}
			}

			static void DisplayGameTip()
				=> IOManager.WriteColored(false, true, ("Plan your movements and press a navigation key (WASD or Arrow keys) once you're ready to start.\nGood luck!", Constants.KeyColor));

			static void GetControlInput()
			{
				int playerLeft    = player.Position.Left;
				int playerTop     = player.Position.Top;
				bool isPlayerIdle = true;

				bool isJumpKeyPressed = false;

				while (isPlayerIdle)
				{
					var input = IOManager.WaitForKeyPress(playAudioEffect: false);
					isJumpKeyPressed = input.Modifiers.HasFlag(BootsOfLeaping.ActivationKey);

					int movementDistance = isJumpKeyPressed ? UpgradeManager.JumpDistanceModifier : 1;

					switch (input.Key)
					{
						case ConsoleKey.W or ConsoleKey.UpArrow:
							playerTop -= movementDistance;
							isPlayerIdle = false;
							break;

						case ConsoleKey.S or ConsoleKey.DownArrow:
							playerTop += movementDistance;
							isPlayerIdle = false;
							break;

						case ConsoleKey.A or ConsoleKey.LeftArrow:
							playerLeft -= movementDistance;
							isPlayerIdle = false;
							break;

						case ConsoleKey.D or ConsoleKey.RightArrow:
							playerLeft += movementDistance;
							isPlayerIdle = false;
							break;
					}
				}

				// Play the 'Boots of Leaping' sound effect if it's equipped, or the normal footstep sound otherwise
				AudioManager.Play(
					UpgradeManager.IsJumpingEnabled && isJumpKeyPressed ?
					AudioEffect.BootsOfLeaping : AudioEffect.Footstep
				);

				// Ensure the player isn't able to bypass the level boundaries
				if ((playerLeft >= 0 && playerLeft <= Constants.WindowWidth - 1) &&
					(playerTop >= WallTopOffset && playerTop <= Constants.WindowHeight - 1))
				{
					player.RedrawAt(new(playerLeft, playerTop));
				}
			}

			static void SetTimersEnabled(bool enabled)
			{
				if (enabled)
				{
					gameStopwatch.Start();
					roundStopwatch.Start();
					roundTimer.Start();
				}
				else
				{
					gameStopwatch.Stop();
					roundStopwatch.Stop();
					roundTimer.Stop();
				}
			}

			static EntityType GetCollisionType()
			{
				if (player.IsAtPosition(exitPoint.Position))
				{
					return EntityType.ExitPoint;
				}
				else if (walls.Any(IsCollidingWithPlayer()))
				{
					return EntityType.Wall;
				}
				else if (bonusPoints.Any(IsCollidingWithPlayer()))
				{
					return EntityType.BonusPoint;
				}
				else if (coins.Any(IsCollidingWithPlayer()))
				{
					return EntityType.Coin;
				}
				else if (firstAidKits.Any(IsCollidingWithPlayer()))
				{
					return EntityType.FirstAidKit;
				}
				else if (hourglasses.Any(IsCollidingWithPlayer()))
				{
					return EntityType.Hourglass;
				}

				return EntityType.None;
			}

			static void HandleCollision(EntityType entity)
			{
				// Added braces for more clarification
				switch (entity)
				{
					case EntityType.ExitPoint:
					{
						currentScore += ExitPointModifier;
						roundState = RoundState.Successful;

						AudioManager.Play(AudioEffect.ExitPoint);
						break;
					}

					case EntityType.Wall:
					{
						if (random.HasRandomChanceOccurred(UpgradeManager.WallCollisionChance))
						{
							// Disallow negative score
							if ((currentScore -= UpgradeManager.WallCollisionPenalty) < 0)
								currentScore = 0;

							if (currentArmors > 0)
							{
								if (--currentArmors <= 0)
									UpgradeManager.RemoveUpgrade(Upgrades.ArmorPoints);
							}
							else if (--currentHearts <= 0)
							{
								roundState = RoundState.Failure;
							}
						}

						var playerPosition = player.Position;

						// Respawn the player at the current round's spawn point
						player.RedrawAt(initialPlayerPosition);

						// Respawn the affected wall(s)
						if (walls.Any(IsAtPosition(playerPosition)))
							walls.First(IsAtPosition(playerPosition)).Draw();

						AudioManager.Play(AudioEffect.WallCollision);
						break;
					}

					case EntityType.BonusPoint:
					{
						currentScore += BonusPointModifier;
						bonusPoints.RemoveAll(IsCollidingWithPlayerPredicate());  // Used to prevent score farming

						AudioManager.Play(AudioEffect.BonusPoint);
						break;
					}

					case EntityType.Coin:
					{
						currentBalance += random.Next(CoinModifierMin, CoinModifierMax + 1);
						coins.RemoveAll(IsCollidingWithPlayerPredicate());

						AudioManager.Play(AudioEffect.Coin);
						break;
					}

					case EntityType.FirstAidKit:
					{
						if (currentHearts < UpgradeManager.MaxHearts)
							++currentHearts;

						firstAidKits.RemoveAll(IsCollidingWithPlayerPredicate());

						AudioManager.Play(AudioEffect.FirstAidKit);
						break;
					}

					case EntityType.Hourglass:
					{
						currentTimeLeft += QuarterOfVictory.ExtendedRoundDurationBonus;
						hourglasses.RemoveAll(IsCollidingWithPlayerPredicate());

						AudioManager.Play(AudioEffect.Hourglass);
						break;
					}
				}
			}

			static void DisplayRoundStats()
			{
				IOManager.StartNewPage(true);

				if (roundState == RoundState.Failure)
				{
					IOManager.WriteColored($"You ran out of {(currentHearts <= 0 ? "hearts" : "time")}!\n", Red, true);
					AudioManager.Play(AudioEffect.Failure);
				}

				IOManager.WriteHeader($"Round {currentRound} statistics");
				Console.WriteLine($"Hearts: {currentHearts}");
				Console.WriteLine($"Score: {currentScore}");
				Console.WriteLine($"Balance: ${currentBalance}");
				Console.WriteLine($"Time elapsed: {roundStopwatch.Elapsed:mm':'ss'.'ff}");

				if (roundState == RoundState.Failure)
					IOManager.WriteColored("\nDon't worry, you can always do better!", Cyan, true);

				IOManager.WaitForKeyPress("\nPress 'Enter' to continue...", true);
			}

			static void DisplayGameStats()
			{
				const int ExtraBonusScoreThreshold = 600;
				const int ExtraBonusAmount         = 250;

				IOManager.StartNewPage(true);
				IOManager.WriteHeader("Game statistics");

				IOManager.WriteColored(false, false,
					($"Total score: {currentScore} {(currentScore > CurrentUser.TopScore ? "(New top score!)" : "")}\n", Gray),
					($"Total cash collected: ${currentBalance}\n", Gray),
					($"Total time elapsed: {gameStopwatch.Elapsed:mm':'ss'.'ff}\n\n", Gray)
				);

				if (roundState == RoundState.Successful)
				{
					var (rating, color) = GetScoreRating();
					IOManager.WriteColored(rating, color, true);

					if (currentScore >= ExtraBonusScoreThreshold)
					{
						IOManager.WriteColored(false, false,
							("You've received an additional ", Gray),
							($"${ExtraBonusAmount} ", Green),
							("bonus for your performance, keep it up!\n", Gray)
						);

						currentBalance += ExtraBonusAmount;
					}
				}

				IOManager.WaitForKeyPress("\nPress 'Enter' to continue to the main menu...", true);

				static (string Rating, ConsoleColor Color) GetScoreRating() =>
					currentScore switch
					{
						>= 600 => ("Wonderful job!", Red),
						>= 500 => ("Awesome work!",  Magenta),
						>= 400 => ("Great job!",     Cyan),
						>= 250 => ("Good job!",      Yellow),
						_      => ("Well done!",     White)
					};
			}

			static void SavePlayerData()
			{
				CurrentUser.Armors = currentArmors;

				if (roundState == RoundState.Successful)
				{
					// Ensure the player can't have more cash than their wallet capacity
					if ((CurrentUser.Balance += currentBalance) > UpgradeManager.WalletCapacity)
						CurrentUser.Balance = UpgradeManager.WalletCapacity;

					if (currentScore > CurrentUser.TopScore)
					{
						// New top score has been reached, update the player's stats
						CurrentUser.TopScore	= currentScore;
						CurrentUser.TimeElapsed = gameStopwatch.Elapsed;
					}
				}
			}

			static Func<Entity, bool> IsAtPosition(Position position) =>         (ent => ent.IsAtPosition(position));
			static Predicate<Entity> IsAtPositionPredicate(Position position) => (ent => ent.IsAtPosition(position));

			static Func<Entity, bool> IsCollidingWithPlayer() =>         (ent => ent.IsAtPosition(player.Position));
			static Predicate<Entity> IsCollidingWithPlayerPredicate() => (ent => ent.IsAtPosition(player.Position));
		}

		private static void RoundTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			const string separator = "    ";
			const ConsoleColor LowValueColor = Red;

			ConsoleColor valueColor = wallColor;
			
			bool isLowOnHearts = currentHearts <= 2;
			bool isRunningOutOfTime = currentTimeLeft <= 10;

			IOManager.ClearLine(0);
			IOManager.ClearLine(1);

			IOManager.WriteColored(false, true,
				($"Round ", Constants.KeyColor), ($"{currentRound}", valueColor), ("/", Constants.KeyColor), ($"{MaxRounds}", valueColor),
				($"{separator}Hearts: ", Constants.KeyColor), (currentHearts.ToString(), isLowOnHearts ? LowValueColor : valueColor),
				($"{separator}Armors: ", Constants.KeyColor), (currentArmors.ToString(), valueColor),
				($"{separator}Score: ", Constants.KeyColor), (currentScore.ToString(), valueColor),
				($"{separator}Balance: ", Constants.KeyColor), ($"${currentBalance}", valueColor),
				($"{separator}Time left: ", Constants.KeyColor),
				($"{(currentTimeLeft > 0 ? --currentTimeLeft : currentTimeLeft)} second{(currentTimeLeft == 1 ? "" : "s")}", isRunningOutOfTime ? LowValueColor : valueColor)
			);

			// Play the corresponding background sound effect(s) if low on health or time
			if (isRunningOutOfTime)
			{
				AudioManager.PlayLooping(BackgroundSound.RunningOutOfTime);
			}
			else if (isLowOnHearts)
				AudioManager.PlayLooping(BackgroundSound.LowOnHearts);
			else
				AudioManager.StopPlayback();

			if (Program.IsDebugModeEnabled)
			{
				IOManager.WriteColored(false, true,
					($"\nPlayer position: ", Constants.KeyColor),
					(player.Position.ToString(), valueColor),
					($"{separator}Player & Exit point dist.: ", Constants.KeyColor),
					($"{player.Position.GetDistanceTo(exitPoint.Position):F1} ", valueColor),
					($"{separator}Random' seed: ", Constants.KeyColor),
					(random.Seed.ToString(), valueColor)
				);
			}

			Console.SetCursorPosition(0, 0);
		}
	}
}
