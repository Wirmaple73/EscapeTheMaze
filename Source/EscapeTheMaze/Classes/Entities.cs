using System;
using EscapeTheMaze.Managers;

namespace EscapeTheMaze.Entities
{
	public abstract class Entity
	{
		public abstract string Name { get; }
		public abstract string Description { get; }

		public Position Position { get; set; }
		public abstract char Character { get; }
		public virtual ConsoleColor Color { get; set; }

		public Entity() { }
		public Entity(Position position) => Position = position;

		public void Draw() => IOManager.WriteAt(Character, Position, Color);
		public void Hide() => IOManager.WriteAt(' ', Position);
		public void RedrawAt(Position position)
		{
			Hide();

			Position = position;
			Draw();
		}

		public bool IsAtPosition(Position position) => Position == position;
	}

	public class Player : Entity
	{
		public override string Name => "Player";
		public override string Description => "Your main, playable character in the game.";

		public override char Character => '☻';
		public override ConsoleColor Color => ConsoleColor.Green;

		public Player() : base() { }
		public Player(Position position) : base(position) { }
	}

	public class ExitPoint : Entity
	{
		public override string Name => "Exit Point";
		public override string Description => $"Your main destination in each round. reaching it will grant you {GameManager.ExitPointModifier} score.";

		public override char Character => '■';
		public override ConsoleColor Color => ConsoleColor.Red;

		public ExitPoint() : base() { }
		public ExitPoint(Position position) : base(position) { }
	}

	public class Wall : Entity
	{
		public override string Name => "Wall";
		public override string Description => $"The main obstacles in the game. Colliding with one deducts a heart, {UpgradeManager.WallCollisionPenalty} score and causes\nyou to respawn, thus be extra careful around those.";

		public override char Character => '▓';
		public override ConsoleColor Color { get; set; } = ConsoleColor.Gray;  // Placeholder color

		public Wall() : base() { }
		public Wall(Position position) : base(position) { }
	}

	public class BonusPoint : Entity
	{
		public override string Name => "Bonus Point";
		public override string Description => $"Collecting one increments your score by {GameManager.BonusPointModifier}.";

		public override char Character => '♦';
		public override ConsoleColor Color => ConsoleColor.Yellow;

		public BonusPoint() : base() { }
		public BonusPoint(Position position) : base(position) { }
	}

	public class Coin : Entity
	{
		public override string Name => "Coin";
		public override string Description => $"Your main source of income, increases your balance by a random value ranging from ${GameManager.CoinModifierMin} to\n${GameManager.CoinModifierMax}.";

		public override char Character => '$';
		public override ConsoleColor Color => ConsoleColor.Green;

		public Coin() : base() { }
		public Coin(Position position) : base(position) { }
	}

	public class FirstAidKit : Entity
	{
		public override string Name => "First Aid Kit";
		public override string Description => $"Restores a heart; however, you won't receive more if you already have {UpgradeManager.MaxHearts}.";

		public override char Character => '♥';
		public override ConsoleColor Color => ConsoleColor.Magenta;

		public FirstAidKit() : base() { }
		public FirstAidKit(Position position) : base(position) { }
	}

	public class Hourglass : Entity
	{
		public override string Name => "Hourglass";
		public override string Description => $"Buys you some more time by incrementing the round time limit by {QuarterOfVictory.ExtendedRoundDurationBonus} seconds.";

		public override char Character => 'ϴ';
		public override ConsoleColor Color => ConsoleColor.Magenta;

		public Hourglass() : base() { }
		public Hourglass(Position position) : base(position) { }
	}
}
