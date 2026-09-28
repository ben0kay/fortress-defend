using Godot;

public partial class Enemy : CharacterBody3D, IDamageable
{
	[Export] public EnemyDefinition Definition { get; set; }

	public int CurrentHealth { get; private set; }

	private Node3D _facing;
	private Building _target;
	private float _attackTimer;
	private float _targetSearchTimer;

	public override void _Ready()
	{
		if (Definition == null)
		{
			GD.PushError($"{Name} has no EnemyDefinition assigned.");
			SetPhysicsProcess(false);
			return;
		}

		_facing = GetNode<Node3D>("Facing");
		CurrentHealth = Definition.MaximumHealth;

		// Layer 3: the player's existing melee Area3D can hit this enemy.
		CollisionLayer = 4;

		// Collide with the floor and buildings.
		CollisionMask = 3;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null || !GodotObject.IsInstanceValid(_target) ||
			_target.CurrentHealth <= 0)
		{
			_targetSearchTimer -= (float)delta;

			if (_targetSearchTimer <= 0.0f)
			{
				FindHeadquarters();
				_targetSearchTimer = 0.5f;
			}

			Velocity = Vector3.Zero;
			return;
		}

		_attackTimer = Mathf.Max(0.0f, _attackTimer - (float)delta);

		Vector3 toTarget = _target.GlobalPosition - GlobalPosition;
		toTarget.Y = 0.0f;

		float distance = toTarget.Length();

		if (distance > 0.01f)
		{
			Vector3 direction = toTarget / distance;
			_facing.Rotation = new Vector3(
				0.0f,
				Mathf.Atan2(-direction.X, -direction.Z),
				0.0f
			);

			if (distance > Definition.AttackRange)
			{
				Velocity = direction * Definition.MoveSpeed;
				MoveAndSlide();
				return;
			}
		}

		Velocity = Vector3.Zero;

		if (_attackTimer > 0.0f)
			return;

		_target.TakeDamage(Definition.AttackDamage);
		_attackTimer = Mathf.Max(0.05f, Definition.AttackInterval);

		GD.Print(
			Definition.DisplayName,
			" hit Headquarters. Health: ",
			_target.CurrentHealth,
			"/",
			_target.MaximumHealth
		);
	}

	private void FindHeadquarters()
	{
		_target = null;
		float nearestDistanceSquared = float.MaxValue;

		foreach (Node child in GetParent().GetChildren())
		{
			if (child is not Building building ||
				building.Definition == null ||
				building.Definition.Role != BuildingRole.Headquarters ||
				building.CurrentHealth <= 0)
				continue;

			float distanceSquared =
				GlobalPosition.DistanceSquaredTo(building.GlobalPosition);

			if (distanceSquared >= nearestDistanceSquared)
				continue;

			_target = building;
			nearestDistanceSquared = distanceSquared;
		}
	}

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || CurrentHealth <= 0)
			return;

		CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

		if (CurrentHealth > 0)
			return;

		GD.Print(Definition.DisplayName, " defeated.");
		QueueFree();
	}
}
