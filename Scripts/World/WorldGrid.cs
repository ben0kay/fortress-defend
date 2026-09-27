using Godot;

public partial class WorldGrid : Node3D
{
	[Export] public float CellSize { get; set; } = 2.0f;

	// Grid cells are centered at Origin, then every CellSize units in X and Z.
	public Vector2I WorldToCell(Vector3 worldPosition)
	{
		Vector3 offset = worldPosition - GlobalPosition;

		return new Vector2I(
			Mathf.RoundToInt(offset.X / CellSize),
			Mathf.RoundToInt(offset.Z / CellSize)
		);
	}

	public Vector3 CellToWorld(Vector2I cell)
	{
		return GlobalPosition + new Vector3(
			cell.X * CellSize,
			0.0f,
			cell.Y * CellSize
		);
	}

	public Vector3 SnapFootprintCenter(Vector3 worldPosition, Vector2I footprint)
	{
		// Odd widths center on a cell; even widths center between cells.
		float xOffset = footprint.X % 2 == 0 ? CellSize * 0.5f : 0.0f;
		float zOffset = footprint.Y % 2 == 0 ? CellSize * 0.5f : 0.0f;

		Vector3 origin = GlobalPosition;

		float x = Mathf.Round((worldPosition.X - origin.X - xOffset) / CellSize)
			* CellSize + origin.X + xOffset;

		float z = Mathf.Round((worldPosition.Z - origin.Z - zOffset) / CellSize)
			* CellSize + origin.Z + zOffset;

		return new Vector3(x, origin.Y, z);
	}
}
