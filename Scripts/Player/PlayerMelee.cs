using Godot;

public partial class PlayerMelee : Area3D
{
	[Export] public int Damage { get; set; } = 1;

	private Player _player;

	public override void _Ready()
	{
		_player = GetNode<Player>("../..");

		// Physics layer 3 contains objects the melee attack can hit.
		CollisionLayer = 0;
		CollisionMask = 4;
		Monitoring = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player.CurrentMode != Player.Mode.Normal ||
			!Input.IsActionJustPressed("melee_attack"))
			return;

		foreach (Node3D body in GetOverlappingBodies())
		{
			if (body is IDamageable target)
				target.TakeDamage(Damage);
		}
	}
}
