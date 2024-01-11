using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using EscapeTheMaze.Entities;
using static System.ConsoleColor;
using static EscapeTheMaze.Managers.UserManager;

namespace EscapeTheMaze.Managers
{
	public static class UIManager
	{
		private static readonly Random random = new();

		private static readonly ConsoleColor[] logoColors = { Yellow, Cyan, Magenta, Green, White };
		private static readonly string[] goodbyeMessages = { "Have a great day!", "See you later!" };

		private static readonly Encoding defaultEncoding = Encoding.UTF8;

		// 'Shell Execute' is apparently required for opening URLs in .NET (https://stackoverflow.com/a/61035650/18954775)
		private static readonly ProcessStartInfo repositoryProcessInfo = new(VersionManager.RepositoryURL) { UseShellExecute = true };

		static UIManager()
		{
			Console.Title = $"Escape The Maze{(Program.IsDebugModeEnabled ? " [Debug mode]" : "")}";

			Console.SetWindowSize(Constants.WindowWidth, Constants.WindowHeight);
			Console.SetBufferSize(Constants.WindowWidth, Constants.WindowHeight + Constants.BufferHeightOffset);

			Console.InputEncoding  = defaultEncoding;
			Console.OutputEncoding = defaultEncoding;
		}

		public static void DisplayLogo()
		{
			IOManager.StartNewPage();
            IOManager.WriteAt(@"
                                        In the name of God


                          ███████ ███████  ██████  █████  ██████  ███████
                          ██      ██      ██      ██   ██ ██   ██ ██
                          █████   ███████ ██      ███████ ██████  █████
                          ██           ██ ██      ██   ██ ██      ██
                          ███████ ███████  ██████ ██   ██ ██      ███████

                                     ████████ ██   ██ ███████
                                        ██    ██   ██ ██ 
                                        ██    ███████ █████
                                        ██    ██   ██ ██
                                        ██    ██   ██ ███████", 0, 0, Gray);

			IOManager.WriteAt(@$"
                                ███╗   ███╗ █████╗ ███████╗███████╗
                                ████╗ ████║██╔══██╗╚══███╔╝██╔════╝
                                ██╔████╔██║███████║  ███╔╝ █████╗
                                ██║╚██╔╝██║██╔══██║ ███╔╝  ██╔══╝
                                ██║ ╚═╝ ██║██║  ██║███████╗███████╗
                                ╚═╝     ╚═╝╚═╝  ╚═╝╚══════╝╚══════╝", 0, 15, logoColors[random.Next(logoColors.Length)]);

			DisplayVersionInfo();
			IOManager.WriteAt("ASCII art: www.patorjk.com", Constants.WindowWidth - 27, Constants.WindowHeight - 2);

			Thread.Sleep(2500);  // Logo screen duration: 2.5s
		}

		public static void DisplayLoginPageAndStoreUser()
		{
			const int maxUsernameLength = 20;
			string username;

			do
			{
				IOManager.StartNewPage(true);

				Console.WriteLine($"Please choose a username to use in-game [Max length: {maxUsernameLength} characters]");
				Console.WriteLine("(Please make sure to enter your exact username if you wish to log in):");

				username = Console.ReadLine().StripExtraWhitespace().Trim();
			} while (string.IsNullOrWhiteSpace(username) || username.Length > maxUsernameLength);

			UserManager.StoreCurrentUserIndex(username);
			UpgradeManager.LoadUserUpgrades();
		}

		public static SelectedMenuItem DisplayMainMenu()
		{
			while (true)
			{
				IOManager.StartNewPage(true);

				DisplayVersionInfo();
				Console.WriteLine($"{(UserManager.IsNewUser ? "Hello there" : "Welcome back")}, {CurrentUser.Username}!\n");

				IOManager.DrawMenu(
					"Play", "Leaderboard", "Upgrade shop", "Change account", "Check for updates", "About", "Exit"
				);

				switch (IOManager.WaitForKeyPress("\nPlease select an option [1-7]: ").KeyChar)
				{
					case '1': return SelectedMenuItem.Play;
					case '2': return SelectedMenuItem.Leaderboard;
					case '3': return SelectedMenuItem.UpgradeShop;
					case '4': return SelectedMenuItem.ChangeAccount;
					case '5': return SelectedMenuItem.CheckForUpdates;
					case '6': return SelectedMenuItem.About;
					case '7': return SelectedMenuItem.Exit;
				}
			}
		}

		public static bool DisplayGameGuide()
		{
			IOManager.StartNewPage();
			IOManager.SetBufferHeight(5);

			IOManager.WriteColored("Welcome to Escape The Maze, a game where you have to reach the 'Exit point' while dodging 'Walls', optionally collecting 'Bonus points' for a score bonus and also 'Coins' which can be spent in the game's upgrade shop.\n", Gray, true, true);
			
			IOManager.WriteHeader("Game entities and their descriptions");
			DisplayEntityInfo<Player>();
			DisplayEntityInfo<ExitPoint>();
			DisplayEntityInfo<Wall>();
			DisplayEntityInfo<BonusPoint>(GameManager.BonusPointSpawnChance, GameManager.MaxBonusPoints);
			DisplayEntityInfo<Coin>(GameManager.CoinSpawnChance, GameManager.MaxCoins);
			DisplayEntityInfo<FirstAidKit>(GameManager.FirstAidKitSpawnChance, GameManager.MaxFirstAidKits);
			DisplayEntityInfo<Hourglass>(GameManager.HourglassSpawnChance, GameManager.MaxHourglasses);

			IOManager.WriteHeader("Game controls");
			IOManager.WriteColored("This game simply utilizes the standard WASD/Arrow keys in order to move your character around.\n", Gray, true, true);

			Console.WriteLine("Press any key to continue, or 'ESC' to return to the main menu...");
			Console.SetCursorPosition(0, 0);

			bool startGame = IOManager.WaitForKeyPress().Key != ConsoleKey.Escape;
			IOManager.ResetBufferHeight();

			return startGame;

			static void DisplayEntityInfo<T>(double spawnChance = 100, int maxOccurrences = 1) where T : Entity, new()
			{
				var entity = new T();
				
				IOManager.WriteColored(false, false, 
					($"[{entity.Character}]", entity.Color),
					($" {entity.Name}", Cyan),
					(": ", Constants.KeyColor),
					(entity.Description, White)
				);

				// Simply print new lines if spawn chances and max occurrences aren't relevant to the passed entity
				if (spawnChance == 100 && maxOccurrences == 1)
				{
					IOManager.WriteColored("\n\n", Constants.KeyColor);
				}
				else
				{
					IOManager.WriteColored(false, false,
						($"\n(Spawn chance: ", Constants.KeyColor),
						($"{spawnChance}%", Constants.ValueColor),
						($", Max occurrences: ", Constants.KeyColor),
						(maxOccurrences.ToString(), Constants.ValueColor),
						(")\n\n", Constants.KeyColor)
					);
				}
			}
		}

		public static void DisplayLeaderboard()
		{
			string line = " " + new string('-', 69);

			IOManager.StartNewPage();
			IOManager.SetBufferHeight(Users.Count);

			IOManager.WriteHeader("Leaderboard");
			Console.WriteLine();

			Console.WriteLine(line);
			Console.WriteLine($" | {"Username",-26} | {"Top score",-9} | {"Balance",-9} | {"Time elapsed",-12} |");
			Console.WriteLine(line);

			// Used to alternate between table row colors
			bool isOddLine = true;

			foreach (var u in Users)
			{
				IOManager.WriteColored($" | {(u.Username == CurrentUser.Username ? $"{u.Username} (You)" : u.Username),-26} | {u.TopScore,-9} | ${u.Balance,-8:N0} | {u.TimeElapsed,-12:mm':'ss'.'ff} |", isOddLine ? DarkGray : Gray, true);
				isOddLine = !isOddLine;
			}

			Console.WriteLine(line);
			IOManager.WaitForKeyPress("\n\nPress any key to return to the main menu...");

			IOManager.ResetBufferHeight();
		}

		public static void DisplayUpgradeShop()
		{
			var upgrades = new IUpgrade[]
			{
				new LevelUpgrade(),
				new ExpandedWallet(),
				new Toughnut(),
				new QuarterOfVictory(),
				new NicerWalls(),
				new BootsOfLeaping(),
				new ArmorPoints()
			};

			while (true)
			{
				IOManager.StartNewPage();
				IOManager.SetBufferHeight(upgrades.Length + 20);

				IOManager.WriteColored("Welcome to the upgrade shop! you can purchase new upgrades for your character here with the coins you collect in-game.\n\n", Gray, wrapText: true);
				IOManager.WriteColored(false, false,
					("Your balance: ", Gray),
					($"${CurrentUser.Balance:N0}", Green),
					($"/", Gray),
					($"${UpgradeManager.WalletCapacity:N0}\n", Green),
					("Your level: ", Constants.KeyColor),
					($"{CurrentUser.Level}\n\n", Constants.ValueColor)
				);

				IOManager.WriteHeader("Available upgrades");

				for (int i = 0; i < upgrades.Length; i++)
				{
					var upgrade = upgrades[i];

					IOManager.WriteColored(false, false,
						("[", Constants.KeyColor),
						($"{i + 1}", Constants.ValueColor),
						("] ", Constants.KeyColor),

						($"{upgrade.Name}", Constants.KeyColor),
						(UpgradeManager.IsUpgradePurchased(upgrade.Upgrade) ? " [Purchased]" : "", Magenta),

						($"\n{upgrade.Description}\n", Cyan),

						("Required level: ", Constants.KeyColor),
						($"{upgrade.RequiredLevel}\n", CurrentUser.Level >= upgrade.RequiredLevel ? Green : Red),

						($"Price: ", Constants.KeyColor),
						($"${upgrade.Price:N0}\n\n", CurrentUser.Balance - upgrade.Price >= 0 ? Green : Red)
					);
				}

				Console.WriteLine($"Choose an upgrade to purchase [1-{upgrades.Length}],\nor press 'ESC' to return to the main menu: ");
				Console.SetCursorPosition(0, 0);

				var inputKey = IOManager.WaitForKeyPress();

				if (inputKey.Key == ConsoleKey.Escape)
					break;

				int index = (int)char.GetNumericValue(inputKey.KeyChar) - 1;

				// Ensure the selected upgrade is valid
				if (upgrades.IsAnyElementAt(index))
				{
					var selectedUpgrade = upgrades[index];

					if (!UpgradeManager.IsUpgradePurchased(selectedUpgrade.Upgrade))
					{
						// Ensure the player has sufficient cash for the purchase
						if (CurrentUser.Balance - selectedUpgrade.Price >= 0)
						{
							if (CurrentUser.Level >= selectedUpgrade.RequiredLevel)
							{
								bool isConfirmed = IOManager.WriteConfirmationMessage(
									("Are you sure you want to purchase the upgrade \"", Gray),
									(selectedUpgrade.Name, Cyan),
									("\" for ", Gray),
									($"${selectedUpgrade.Price:N0}", Green),
									("?", Gray)
								);

								if (isConfirmed)
								{
									UpgradeManager.AddUpgrade(selectedUpgrade.Upgrade);
									CurrentUser.Balance -= selectedUpgrade.Price;

									AudioManager.Play(AudioEffect.UpgradePurchase);

									IOManager.WriteColored(true, false,
										("Successfully purchased the upgrade \"", Gray),
										(selectedUpgrade.Name, Cyan),
										("\" for ", Gray),
										($"${selectedUpgrade.Price:N0}", Green),
										(".\n", Gray)
									);

									// Handle 'special' upgrades
									switch (selectedUpgrade.Upgrade)
									{
										case Upgrades.ArmorPoints:
											CurrentUser.Armors = ArmorPoints.ArmorCount;
											break;

										case Upgrades.LevelUpgrade:
											// Ensure the player can purchase another level upgrade later
											UpgradeManager.RemoveUpgrade(Upgrades.LevelUpgrade);
											++CurrentUser.Level;
											break;
									}
								}
							}
							else
							{
								DisplayErrorMessage("Your current level does not meet this upgrade's minimum required level.");
							}
						}
						else
						{
							DisplayErrorMessage("You don't have sufficient funds to purchase this upgrade.");
						}
					}
					else
					{
						DisplayErrorMessage("This upgrade is already purchased.");
					}
				}
				else
				{
					DisplayErrorMessage("Invalid option selected, please try again.");
				}
			}

			IOManager.ResetBufferHeight();

			static void DisplayErrorMessage(string message)
				=> IOManager.WriteColored(true, false, ($"{message}\n", Red));
		}

		public static void DisplayUpdatePage()
		{
			IOManager.StartNewPage(true);
			Console.WriteLine("Checking for new updates, please be patient...\n");

			var (lookupState, message) = VersionManager.VersionLookupState;

			switch (lookupState)
			{
				case VersionLookupState.UpToDate:
					IOManager.WriteColored("Looks like you are running the latest version of the game.", Green, true);
					break;

				case VersionLookupState.UpdateAvailable:
					IOManager.WriteColored("There is a new update available!", Green, true);
					IOManager.WriteColored(VersionManager.RepositoryURL, Cyan, true);

					IOManager.WriteColored("\nPress 'Enter' to navigate to the URL above, or any other key to return to the main menu...", Constants.KeyColor, wrapText: true);

					if (IOManager.WaitForKeyPress().Key == ConsoleKey.Enter)
					{
						try
						{
							Process.Start(repositoryProcessInfo);
						}
						catch { }
					}

					return;

				case VersionLookupState.ConnectionError:
					IOManager.WriteColored("There was an error trying to fetch data from the target URL. Please make sure you are properly connected to the internet and try again.\n", Red, true, true);
                    Console.WriteLine("Error details:");
					IOManager.WriteColored(message, Red, true, true);

					break;
			}

			IOManager.WaitForKeyPress("\n\nPress any key to return to the main menu...");
		}

		public static void DisplayAboutPage()
		{
			IOManager.StartNewPage(true);

			IOManager.WriteColored(false, false,
				($"Escape The Maze {VersionManager.CurrentVersion}\n", White),
				($"Build date: {VersionManager.BuildDate:yyyy/MM/dd}\n", Constants.KeyColor),
				("https://github.com/Wirmaple73/\n\n", Cyan),
				("All downloaded audio effects belong to their own respective owners, I do not own any of them.", Constants.KeyColor)
			);

			IOManager.WaitForKeyPress("\n\nPress any key to return to the main menu...");
		}

		public static void DisplayExitPage()
		{
			IOManager.StartNewPage();
			IOManager.WriteColored(false, true, (goodbyeMessages[random.Next(goodbyeMessages.Length)], Cyan));

			Thread.Sleep(1200);
		}

		private static void DisplayVersionInfo()
			=> IOManager.WriteAt($"v{VersionManager.CurrentVersion} ({VersionManager.BuildDate:yyyy/MM/dd})" +
				$"{(Program.IsDebugModeEnabled ? " [Debug mode]" : "")}",
				1, Constants.WindowHeight - 2);
	}
}
