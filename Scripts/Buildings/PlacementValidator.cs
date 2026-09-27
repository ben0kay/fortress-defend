using Godot;

public sealed class PlacementValidator
{
	// Physics layer 2 is the Placement Blocker layer.
	private const uint PlacementBlockerLayer = 2;

	private readonly WorldGrid _worldGrid;
	private readonly BoxShape3D _footprintShape = new();
	private readonly PhysicsShapeQueryParameters3D _query = new();

	public PlacementValidator(WorldGrid worldGrid)
	{
		_worldGrid = worldGrid;

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
	if (definition == null)
		return false;

	// Check the number of existing buildings of this type.
	if (_worldGrid.CountBuildings(definition.Key) >= definition.MaximumInstances)
		return false;

	// Check cells already reserved by buildings.
	if (!_worldGrid.CanOccupy(position, definition.FootprintCells))
		return false;

	// Check the player, wall, and other physics blockers.
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
