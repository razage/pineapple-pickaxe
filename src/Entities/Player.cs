using Godot;

public class Player : KinematicBody2D
{
	[Export] private float speed;

	public override void _Ready()
	{
		Position = GetNode<Position2D>("../SpawnPoint").Position;
	}

	public override void _PhysicsProcess(float delta)
	{
		Vector2 currentDirection = Vector2.Zero;

		currentDirection.y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
		currentDirection.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		currentDirection = currentDirection.Normalized();

		Move(currentDirection);
	}

	public void Move(Vector2 direction)
	{
		if (direction != Vector2.Zero)
		{
			MoveAndSlide(direction * speed);
		}
	}
}
