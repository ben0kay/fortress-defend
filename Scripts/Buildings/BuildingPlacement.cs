using Godot;

public partial class BuildingPlacement : Node3D
{
	[Export] public int GridCellsAcross { get; set; } = 15;

	private Camera3D _playerCamera;
	private WorldGrid _worldGrid;
	private BuildingDefinition _selectedBuilding;
	private Node3D _preview;
	private MeshInstance3D _gridVisual;

	public override void _Ready()
	{
		// GameMaker equivalent: Create event.
		_playerCamera = GetNode<Camera3D>("../Player/Camera3D");
		_worldGrid = GetNode<WorldGrid>("../WorldGrid");
	}

	public override void _Process(double delta)
	{
		// GameMaker equivalent: Step event. No work outside placement mode.
		if (_preview == null)
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = _playerCamera.ProjectRayOrigin(mousePosition);
		Vector3 rayDirection = _playerCamera.ProjectRayNormal(mousePosition);

		// Current sandbox ground is level with the world grid.
		Plane ground = new(Vector3.Up, _worldGrid.GlobalPosition.Y);
		Vector3? hitPosition = ground.IntersectsRay(rayOrigin, rayDirection);

		if (hitPosition == null)
		{
			_preview.Hide();
			_gridVisual.Hide();
			return;
		}

		// The building snaps according to its footprint's odd/even dimensions.
		_preview.GlobalPosition = _worldGrid.SnapFootprintCenter(
			hitPosition.Value,
			_selectedBuilding.FootprintCells
		);

		// The visible patch stays aligned with WORLD cells, even for a 2x2 building.
		Vector2I nearestCell = _worldGrid.WorldToCell(hitPosition.Value);
		_gridVisual.GlobalPosition = _worldGrid.CellToWorld(nearestCell);

		_preview.Show();
		_gridVisual.Show();
	}

	public void BeginPlacement(BuildingDefinition definition)
	{
		CancelPlacement();

		if (definition == null || definition.Scene == null)
			return;

		_selectedBuilding = definition;
		CreatePreview();
		CreateGridVisual();
	}

	private void CreatePreview()
	{
		_preview = _selectedBuilding.Scene.Instantiate<Node3D>();
		AddChild(_preview);

		// A preview is visual only; it cannot block the player.
		if (_preview is CollisionObject3D collisionObject)
		{
			collisionObject.CollisionLayer = 0;
			collisionObject.CollisionMask = 0;
		}

		CollisionShape3D collisionShape =
			_preview.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");

		if (collisionShape != null)
			collisionShape.Disabled = true;

		MeshInstance3D mesh =
			_preview.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

		if (mesh != null)
		{
			mesh.MaterialOverride = new StandardMaterial3D
			{
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				AlbedoColor = new Color(0.2f, 0.9f, 1.0f, 0.45f)
			};
		}
	}

	private void CreateGridVisual()
	{
		// Create the lines once. Each frame we only move this mesh.
		int cells = Mathf.Max(
			GridCellsAcross,
			Mathf.Max(_selectedBuilding.FootprintCells.X,
					  _selectedBuilding.FootprintCells.Y) + 4
		);

		if (cells % 2 == 0)
			cells++;

		float cellSize = _worldGrid.CellSize;
		float halfWidth = cells * cellSize * 0.5f;
		ImmediateMesh lines = new();

		lines.SurfaceBegin(Mesh.PrimitiveType.Lines);

		for (int line = 0; line <= cells; line++)
		{
			float coordinate = -halfWidth + line * cellSize;

			lines.SurfaceAddVertex(new Vector3(coordinate, 0.04f, -halfWidth));
			lines.SurfaceAddVertex(new Vector3(coordinate, 0.04f, halfWidth));

			lines.SurfaceAddVertex(new Vector3(-halfWidth, 0.04f, coordinate));
			lines.SurfaceAddVertex(new Vector3(halfWidth, 0.04f, coordinate));
		}

		lines.SurfaceEnd();

		_gridVisual = new MeshInstance3D
		{
			Mesh = lines,
			MaterialOverride = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				AlbedoColor = new Color(0.5f, 0.85f, 1.0f, 0.55f)
			}
		};

		AddChild(_gridVisual);
	}

	public void CancelPlacement()
	{
		_selectedBuilding = null;

		if (_preview != null)
		{
			_preview.QueueFree();
			_preview = null;
		}

		if (_gridVisual != null)
		{
			_gridVisual.QueueFree();
			_gridVisual = null;
		}
	}
}
