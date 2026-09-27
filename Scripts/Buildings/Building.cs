using Godot;
using System;

public partial class Building : StaticBody3D
{
	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maximumHealth);

	[Signal]
	public delegate void DestroyedEventHandler(Building building);

	[Export] public BuildingDefinition Definition { get; set; }

	// Placement sets this BEFORE adding the preview to the scene tree.
	public bool IsPreview { get; set; }

	public int MaximumHealth { get; private set; }
	public int CurrentHealth { get; private set; }

	private WorldGrid _worldGrid;
	private bool _hasReservedCells;

	public override void _Ready()
	{
		// A ghost is never a real building and never reserves cells.
		if (IsPreview)
			return;

		if (Definition == null)
		{
			GD.PushError($"{Name} has no BuildingDefinition assigned.");
			return;
		}

		MaximumHealth = Definition.MaximumHealth;
		CurrentHealth = MaximumHealth;

		_worldGrid = GetNode<WorldGrid>("../WorldGrid");

		GlobalPosition = _worldGrid.SnapFootprintCenter(
			GlobalPosition,
			Definition.FootprintCells
		);

		_hasReservedCells = _worldGrid.TryOccupy(
			this,
			GlobalPosition,
			Definition.FootprintCells
		);

		if (!_hasReservedCells)
			GD.PushError($"{Name} could not reserve its building footprint.");
	}

	public override void _ExitTree()
	{
		// GameMaker equivalent: Clean Up event.
		if (_hasReservedCells)
			_worldGrid.Release(this);
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
