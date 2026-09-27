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
		Vector2I footprintCells)
	{
		// Buildings reserve world grid cells.
		if (!_worldGrid.CanOccupy(position, footprintCells))
			return false;

		// Check the same footprint against the player, walls, and rocks.
		// The small inset allows objects to touch at the cell boundary.
		_footprintShape.Size = new Vector3(
			footprintCells.X * _worldGrid.CellSize - 0.1f,
			2.0f,
			footprintCells.Y * _worldGrid.CellSize - 0.1f
		);

		// The box starts at ground level and extends two units upward.
		_query.Transform = new Transform3D(
			Basis.Identity,
			position + Vector3.Up
		);

		return space.IntersectShape(_query, 1).Count == 0;
	}
}
