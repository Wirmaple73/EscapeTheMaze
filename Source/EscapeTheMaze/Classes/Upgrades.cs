using System;
using EscapeTheMaze.Managers;

namespace EscapeTheMaze
{
	public interface IUpgrade
	{
		Upgrades Upgrade { get; }
		string Name { get; }
		string Description { get; }
		int Price { get; }
		int RequiredLevel { get; }
	}

	public readonly struct LevelUpgrade : IUpgrade
	{
		private const int CashLevelMultiplier = 1000;
		private static int CashRequiredForLevelUp => UserManager.CurrentUser.Level * CashLevelMultiplier;

		public Upgrades Upgrade => Upgrades.LevelUpgrade;
		public string Name => "Level Upgrade";
		public string Description => $"Levels up your character to level {UserManager.CurrentUser.Level + 1}.";
		public int Price => CashRequiredForLevelUp;
		public int RequiredLevel => 1;
	}

	public readonly struct ExpandedWallet : IUpgrade
	{
		public const int NormalCapacity   = 2500;
		public const int ExtendedCapacity = 5000;

		public Upgrades Upgrade => Upgrades.ExpandedWallet;
		public string Name => "Expanded Wallet";
		public string Description => $"Expands your maximum wallet capacity to ${ExtendedCapacity:N0}.";
		public int Price => NormalCapacity;
		public int RequiredLevel => 2;
	}

	public readonly struct Toughnut : IUpgrade
	{
		public const int NormalCapacity = 3;
		public const int MaxCapacity    = 5;

		public Upgrades Upgrade => Upgrades.Toughnut;
		public string Name => "Toughnut";
		public string Description => $"Increases your maximum heart capacity to {MaxCapacity}.";
		public int Price => 4000;
		public int RequiredLevel => 3;
	}

	public readonly struct QuarterOfVictory : IUpgrade
	{
		public const int DefaultRoundDuration  = 45;
		public const int ExtendedRoundDuration = 60;

		public const int ExtendedRoundDurationBonus = ExtendedRoundDuration - DefaultRoundDuration;

		public Upgrades Upgrade => Upgrades.QuarterOfVictory;
		public string Name => "The Quarter of Victory";
		public string Description => $"Increases the round time limit to {ExtendedRoundDuration} seconds.";
		public int Price => 3750;
		public int RequiredLevel => 3;
	}

	public readonly struct NicerWalls : IUpgrade
	{
		public const int CollisionPenalty      = 20;
		public const double NewCollisionChance = 70;

		public Upgrades Upgrade => Upgrades.NicerWalls;
		public string Name => "Nicer Walls";
		public string Description => $"Reduces the wall collision chance to {NewCollisionChance}% and halves the score penalty (though, it won't prevent\nrespawning).";
		public int Price => 3250;
		public int RequiredLevel => 3;
	}

	public readonly struct BootsOfLeaping : IUpgrade
	{
		public const int JumpDistanceModifier = 2;
		public const ConsoleModifiers ActivationKey = ConsoleModifiers.Shift;

		public Upgrades Upgrade => Upgrades.BootsOfLeaping;
		public string Name => "The Boots of Leaping";
		public string Description => $"Hold '{ActivationKey}' along with the navigation keys (WASD only!) to double your character's movement speed\nand additionally, jump over walls!";
		public int Price => ExpandedWallet.ExtendedCapacity;
		public int RequiredLevel => 5;
	}

	public readonly struct ArmorPoints : IUpgrade
	{
		public const int ArmorCount = 3;

		public Upgrades Upgrade => Upgrades.ArmorPoints;
		public string Name => $"{ArmorCount}x Armor Points";
		public string Description => $"Armor points which are capable of absorbing the inflicted wall collision damage instead of your\nhearts for up to {ArmorCount} times (You can purchase this upgrade again once you run out of armor points).";
		public int Price => ArmorCount * 250;  // $250 per armor point
		public int RequiredLevel => 2;
	}
}
