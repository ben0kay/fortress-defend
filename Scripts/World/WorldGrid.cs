using Godot;
using System.Collections.Generic;

public partial class WorldGrid : Node3D
{
	[Export] public float CellSize { get; set; } = 2.0f;

	// A missing cell is free. An occupied cell records the object that owns it.
	private readonly Dictionary<Vector2I, Node3D> _cellOwners = new();
	private readonly Dictionary<Node3D, List<Vector2I>> _ownerCells = new();
	private readonly Dictionary<string, int> _buildingCounts = new();

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
		// Odd dimensions center on a cell; even dimensions center between cells.
		float xOffset = footprint.X % 2 == 0 ? CellSize * 0.5f : 0.0f;
		float zOffset = footprint.Y % 2 == 0 ? CellSize * 0.5f : 0.0f;
		Vector3 origin = GlobalPosition;

		float x = Mathf.Round((worldPosition.X - origin.X - xOffset) / CellSize)
			* CellSize + origin.X + xOffset;

		float z = Mathf.Round((worldPosition.Z - origin.Z - zOffset) / CellSize)
			* CellSize + origin.Z + zOffset;

		return new Vector3(x, origin.Y, z);
	}

	public List<Vector2I> GetFootprintCells(Vector3 snappedCenter, Vector2I footprint)
	{
		List<Vector2I> cells = new();

		if (footprint.X <= 0 || footprint.Y <= 0)
			return cells;

		// Find the lower-left cell of the building's footprint.
		float centerX = (snappedCenter.X - GlobalPosition.X) / CellSize;
		float centerZ = (snappedCenter.Z - GlobalPosition.Z) / CellSize;

		int firstX = Mathf.RoundToInt(centerX - (footprint.X - 1) * 0.5f);
		int firstZ = Mathf.RoundToInt(centerZ - (footprint.Y - 1) * 0.5f);

		for (int x = 0; x < footprint.X; x++)
		{
			for (int z = 0; z < footprint.Y; z++)
				cells.Add(new Vector2I(firstX + x, firstZ + z));
		}

		return cells;
	}

	public bool CanOccupy(Vector3 snappedCenter, Vector2I footprint)
	{
		List<Vector2I> cells = GetFootprintCells(snappedCenter, footprint);

		if (cells.Count == 0)
			return false;

		foreach (Vector2I cell in cells)
		{
			if (_cellOwners.ContainsKey(cell))
				return false;
		}

		return true;
	}

	public bool TryOccupy(Node3D owner, Vector3 snappedCenter, Vector2I footprint)
	{
		if (owner == null || _ownerCells.ContainsKey(owner))
			return false;

		List<Vector2I> cells = GetFootprintCells(snappedCenter, footprint);

		if (cells.Count == 0)
			return false;

		// Check the whole footprint before changing any cells.
		foreach (Vector2I cell in cells)
		{
			if (_cellOwners.ContainsKey(cell))
				return false;
		}

		foreach (Vector2I cell in cells)
			_cellOwners[cell] = owner;

		_ownerCells[owner] = cells;

		if (owner is Building building && building.Definition != null)
		{
			string key = building.Definition.Key;
			_buildingCounts[key] = CountBuildings(key) + 1;
		}

		return true;
	}

	public void Release(Node3D owner)
	{
		if (!_ownerCells.TryGetValue(owner, out List<Vector2I> cells))
			return;

		foreach (Vector2I cell in cells)
			_cellOwners.Remove(cell);

		_ownerCells.Remove(owner);

		if (owner is Building building && building.Definition != null)
		{
			string key = building.Definition.Key;
			int remaining = CountBuildings(key) - 1;

			if (remaining > 0)
				_buildingCounts[key] = remaining;
			else
				_buildingCounts.Remove(key);
		}
	}

	public int CountBuildings(string definitionKey)
	{
		return _buildingCounts.TryGetValue(definitionKey, out int count)
			? count
			: 0;
	}
}
