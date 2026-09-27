using Godot;

public partial class Player : CharacterBody3D
{
	public enum Mode
	{
		Normal,
		BuildMenu,
		Placing
	}

	[Export] public float MoveSpeed { get; set; } = 5.0f;

	public PlayerGathering Gathering { get; private set; }
	public Mode CurrentMode { get; set; } = Mode.Normal;

	private AnimationPlayer _walkAnimation;

	public override void _Ready()
	{
		Gathering = GetNode<PlayerGathering>("Gathering");
		_walkAnimation = GetNode<AnimationPlayer>(
            "Facing/PLAYER_NEW/AnimationPlayer"
		);

		_walkAnimation.Play("Walk");
		_walkAnimation.Pause();
		_walkAnimation.Seek(0.0, true);
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyMovement();
		ApplyGravity(delta);
		MoveAndSlide();

		bool isMoving =
			new Vector2(Velocity.X, Velocity.Z).LengthSquared() > 0.01f;

		if (isMoving)
		{
			if (!_walkAnimation.IsPlaying())
				_walkAnimation.Play("Walk");
		}
		else if (_walkAnimation.IsPlaying())
		{
			_walkAnimation.Pause();
			_walkAnimation.Seek(0.0, true);
		}
	}

	private void ApplyMovement()
	{
		Vector2 inputDirection = Input.GetVector(
			"move_left",
			"move_right",
			"move_up",
            "move_down"
		);

		Vector3 movement = Velocity;
		movement.X = inputDirection.X * MoveSpeed;
		movement.Z = inputDirection.Y * MoveSpeed;
		Velocity = movement;
	}

	private void ApplyGravity(double delta)
	{
		Vector3 movement = Velocity;

		if (IsOnFloor())
			movement.Y = 0.0f;
		else
			movement += GetGravity() * (float)delta;

		Velocity = movement;
	}
}
