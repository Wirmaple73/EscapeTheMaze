using static EscapeTheMaze.Managers.UserManager;

namespace EscapeTheMaze.Managers
{
    public static class UpgradeManager
    {
        public static int WalletCapacity { get; private set; }

        public static int MaxHearts { get; private set; }
        public static int RoundDuration { get; private set; }

        public static int WallCollisionPenalty { get; private set; }
        public static double WallCollisionChance { get; private set; }

        public static bool IsJumpingEnabled { get; private set; }
        public static int JumpDistanceModifier { get; private set; }

        public static void LoadUserUpgrades()
        {
            WalletCapacity = IsUpgradePurchased(Upgrades.ExpandedWallet) ?
                ExpandedWallet.ExtendedCapacity : ExpandedWallet.NormalCapacity;

            MaxHearts = IsUpgradePurchased(Upgrades.Toughnut) ? Toughnut.MaxCapacity : Toughnut.NormalCapacity;

            RoundDuration = IsUpgradePurchased(Upgrades.QuarterOfVictory) ?
                QuarterOfVictory.ExtendedRoundDuration : QuarterOfVictory.DefaultRoundDuration;

            WallCollisionPenalty = IsUpgradePurchased(Upgrades.NicerWalls) ?
                NicerWalls.CollisionPenalty / 2 : NicerWalls.CollisionPenalty;

            WallCollisionChance = IsUpgradePurchased(Upgrades.NicerWalls) ? NicerWalls.NewCollisionChance : 100;

            IsJumpingEnabled = IsUpgradePurchased(Upgrades.BootsOfLeaping);
            JumpDistanceModifier = IsJumpingEnabled ? BootsOfLeaping.JumpDistanceModifier : 1;
        }

        public static void AddUpgrade(Upgrades upgrade)
        {
            CurrentUser.Upgrades |= upgrade;
            LoadUserUpgrades();
        }

        public static void RemoveUpgrade(Upgrades upgrade)
        {
            CurrentUser.Upgrades &= ~upgrade;
            LoadUserUpgrades();
        }

        public static bool IsUpgradePurchased(Upgrades upgrade) => CurrentUser.Upgrades.HasFlag(upgrade);
    }
}
