using Godot;

public sealed class PlacementValidator
{
	private const uint PlacementBlockerLayer = 2;

	private readonly WorldGrid _worldGrid;
	private readonly ResourceInventory _inventory;
	private readonly BoxShape3D _footprintShape = new();
	private readonly PhysicsShapeQueryParameters3D _query = new();

	public PlacementValidator(
		WorldGrid worldGrid,
		ResourceInventory inventory)
	{
		_worldGrid = worldGrid;
		_inventory = inventory;

		_query.Shape = _footprintShape;
		_query.CollisionMask = PlacementBlockerLayer;
		_query.CollideWithBodies = true;
		_query.CollideWithAreas = false;
	}

	public bool CanPlace(
		PhysicsDirectSpaceState3D space,
		Vector3 position,
		BuildingDefinition definition)
	{
		if (definition == null || !_inventory.CanAfford(definition))
			return false;

		if (_worldGrid.CountBuildings(definition.Key) >=
			definition.MaximumInstances)
			return false;

		if (!_worldGrid.CanOccupy(position, definition.FootprintCells))
			return false;

		_footprintShape.Size = new Vector3(
			definition.FootprintCells.X * _worldGrid.CellSize - 0.1f,
			2.0f,
			definition.FootprintCells.Y * _worldGrid.CellSize - 0.1f
		);

		_query.Transform = new Transform3D(
			Basis.Identity,
			position + Vector3.Up
		);

		return space.IntersectShape(_query, 1).Count == 0;
	}
}
