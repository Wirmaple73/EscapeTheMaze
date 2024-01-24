using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EscapeTheMaze.Managers;

namespace EscapeTheMaze
{
	public static class ExtensionMethods
	{
		// Source (slightly modified):
		// https://stackoverflow.com/a/29689349
		public static string Wrap(this string value)
		{
			var words = value.Split(' ');
			var lines = words.Skip(1).Aggregate(words.Take(1).ToList(), (l, w) =>
			{
				if (l.Last().Length + w.Length >= Constants.WindowWidth - 1)
					l.Add(w);
				else
					l[^1] += " " + w;

				return l;
			});

			return string.Join('\n', lines);
		}

		public static string StripExtraWhitespace(this string value) => Regex.Replace(value, @"\s+", " ");

		public static bool IsAnyElementAt<T>(this IList<T> values, int index) => values.ElementAtOrDefault(index) is not null;
	}

	public class GameRandom : Random
	{
		public int Seed { get; }

		public GameRandom(int seed) : base(seed) => Seed = seed;  // Used to store the seed explicitly

		public Position NextPosition(bool fetchSafePosition = false)
		{
			return GetRandomPosition(fetchSafePosition ? 2 : 1);

			Position GetRandomPosition(int offset)
				=> new(base.Next(offset, Constants.WindowWidth - offset),
					   base.Next(offset + GameManager.WallTopOffset, Constants.WindowHeight - offset));
		}

		public Position NextPositionFarFrom(Position target, bool fetchSafePosition = false)
		{
			Position newPosition;

			do
			{
				newPosition = NextPosition(fetchSafePosition);
			} while (target.IsCloseTo(newPosition));

			return newPosition;
		}

		// Source:
		// https://stackoverflow.com/a/37858669
		public bool HasRandomChanceOccurred(double chance) => (base.Next(100) + base.NextDouble()) < chance;
	}

	public class User
	{
		public string Username { get; init; }
		public int Level { get; set; }
		public int TopScore { get; set; }
		public int Armors { get; set; }
		public int Balance { get; set; }
		public Upgrades Upgrades { get; set; }
		public TimeSpan TimeElapsed { get; set; }
		public bool HasEverPlayed => TimeElapsed > TimeSpan.Zero;

		public User() { }
		public User(string username) : this(username, 1, 0, 0, 0, Upgrades.None, TimeSpan.Zero) { }
		public User(string username, int level, int topScore, int armors, int balance, Upgrades upgrades, TimeSpan timeElapsed)
		{
			EnsureArgumentIsGreaterThan(level, 1, nameof(level));
			EnsureArgumentIsGreaterThan(topScore, 0, nameof(topScore));
			EnsureArgumentIsGreaterThan(armors, 0, nameof(armors));
			EnsureArgumentIsGreaterThan(balance, 0, nameof(balance));

			Username    = username;
			Level       = level;
			TopScore    = topScore;
			Armors      = armors;
			Balance     = balance;
			Upgrades    = upgrades;
			TimeElapsed = timeElapsed;

			static void EnsureArgumentIsGreaterThan(int argument, int value, string argumentName)
			{
				if (argument < value)
					throw new ArgumentOutOfRangeException(argumentName, $"'{argumentName}' is out of range.");
			}
		}
	}

	public readonly struct Position : IEquatable<Position>
	{
		public static readonly Position Empty = new(0, 0);

		public int Left { get; init; }
		public int Top { get; init; }

		public bool IsEmpty => this == Empty;

		private const double CloseDistanceThreshold = 25;

		public Position(int left, int top)
		{
			if (left < 0)
				throw new ArgumentOutOfRangeException(nameof(left), "'Left' must be >= 0.");

			if (top < 0)
				throw new ArgumentOutOfRangeException(nameof(top), "'Top' must be >= 0.");

			Left = left;
			Top = top;
		}

		public double GetDistanceTo(Position other)
			=> Math.Sqrt(Math.Pow(other.Left - Left, 2) + Math.Pow(other.Top - Top, 2));  // The distance formula

		public bool IsCloseTo(Position position) => GetDistanceTo(position) <= CloseDistanceThreshold;

		public static bool operator ==(Position left, Position right) => left.Equals(right);
		public static bool operator !=(Position left, Position right) => !left.Equals(right);

		public override bool Equals(object obj) => obj is Position position && Equals(position);
		public bool Equals(Position other) => Left == other.Left && Top == other.Top;

		public override int GetHashCode() => base.GetHashCode();  // Doesn't matter for now
		public override string ToString() => $"({Left}, {Top})";
	}

	public enum SelectedMenuItem
	{
		Play, Leaderboard, UpgradeShop, ChangeAccount, CheckForUpdates, About, Exit, ImportUsers, ExportUsers
	}

	public enum AudioEffect
	{
		MenuNavigation, UpgradePurchase, Failure, BootsOfLeaping, Footstep, ExitPoint,
		WallCollision, BonusPoint, Coin, FirstAidKit, Hourglass
	}

	public enum BackgroundSound
	{
		RunningOutOfTime, LowOnHearts
	}

	public enum EntityType
	{
		Player, Wall, ExitPoint, BonusPoint, Coin, FirstAidKit, Hourglass, None
	}

	public enum RoundState
	{
		Ongoing, Successful, Failure
	}

	[Flags]
	public enum Upgrades
	{
		None			 = 0,
		LevelUpgrade	 = 1,
		ExpandedWallet	 = 2,
		Toughnut		 = 4,
		QuarterOfVictory = 8,
		NicerWalls		 = 16,
		BootsOfLeaping	 = 32,
		ArmorPoints		 = 64
	}

	public enum VersionLookupState
	{
		UpToDate, UpdateAvailable, ConnectionError
	}
}
