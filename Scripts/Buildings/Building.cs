using Godot;
using System;

public partial class Building : StaticBody3D
{
	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maximumHealth);

	[Signal]
	public delegate void DestroyedEventHandler(Building building);

	[Export] public int MaximumHealth { get; set; } = 100;

	public int CurrentHealth { get; private set; }

	public override void _Ready()
	{
		// GameMaker equivalent: Create event.
		CurrentHealth = MaximumHealth;
	}

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || CurrentHealth <= 0)
			return;

		CurrentHealth = Math.Max(CurrentHealth - amount, 0);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaximumHealth);

		if (CurrentHealth == 0)
			EmitSignal(SignalName.Destroyed, this);
	}
}
