using Godot;

namespace PineapplePickaxe.Generators.Utilities
{
	public static class Utilities
	{
		private static RandomNumberGenerator rng = new RandomNumberGenerator();
		private static Vector2[] directions = new Vector2[] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };

		public static Vector2 GetRandomDirection()
		{
			rng.Randomize();
			return directions[rng.RandiRange(0, 3)];
		}
	}
	public class Walker : Reference
	{
		public Vector2 position;
		public Vector2 currentDirection;

		public Walker(Vector2 startingPos, Vector2 startingDirection)
		{
			position = startingPos;
			currentDirection = startingDirection;
		}
	}
}