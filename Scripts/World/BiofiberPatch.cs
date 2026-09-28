using Godot;

public partial class BiofiberPatch : StaticBody3D, IDamageable
{
	[Export] public int MaximumHealth { get; set; } = 2;
	[Export] public int BiofiberYield { get; set; } = 3;

	private int _currentHealth;
	private ResourceInventory _inventory;

	public override void _Ready()
	{
		_currentHealth = MaximumHealth;
		_inventory = GetNode<ResourceInventory>("../ResourceInventory");

		// Layer 2 blocks building placement; layer 3 receives melee hits.
		// Neither layer blocks the player's current movement mask.
		CollisionLayer = 6;
		CollisionMask = 0;
	}

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || _currentHealth <= 0)
			return;

		_currentHealth = Mathf.Max(0, _currentHealth - amount);

		if (_currentHealth > 0)
			return;

		if (_inventory.Add("biofiber", BiofiberYield))
		{
			GD.Print(
				"Biofiber: ",
				_inventory.GetAmount("biofiber")
			);
		}

		QueueFree();
	}
}
