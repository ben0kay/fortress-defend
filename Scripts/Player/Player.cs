using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
	public enum Mode
	{
		Normal,
		BuildMenu,
		Placing
	}

	[Signal]
	public delegate void ResourceChangedEventHandler(string resourceKey, int newAmount);

	[Export] public float MoveSpeed { get; set; } = 5.0f;
	[Export] public AnimationPlayer WalkAnimation { get; set; }


	public PlayerGathering Gathering { get; private set; }
	public Mode CurrentMode { get; set; } = Mode.Normal;

	// Add more resource keys here as the game grows.
	public Dictionary<string, int> Resources { get; } = new()
	{
		["carbon"] = 0
	};



	public override void _Ready()
	{
		Gathering = GetNode<PlayerGathering>("Gathering");



		WalkAnimation.Play("Walk");
		WalkAnimation.Pause();
		WalkAnimation.Seek(0.0, true);
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyMovement();
		ApplyGravity(delta);
		MoveAndSlide();

		bool isMoving = new Vector2(Velocity.X, Velocity.Z).LengthSquared() > 0.01f;

		if (isMoving)
		{
			if (!WalkAnimation.IsPlaying())
				WalkAnimation.Play("Walk");
		}
		else if (WalkAnimation.IsPlaying())
		{
			WalkAnimation.Pause();
			WalkAnimation.Seek(0.0, true);
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

	public void AddResource(string resourceKey, int amount)
	{
		if (amount <= 0)
			return;

		Resources.TryGetValue(resourceKey, out int currentAmount);
		int newAmount = currentAmount + amount;

		Resources[resourceKey] = newAmount;
		EmitSignal(SignalName.ResourceChanged, resourceKey, newAmount);

		// Temporary feedback alongside the HUD.
		GD.Print(resourceKey, ": ", newAmount);
	}
}
