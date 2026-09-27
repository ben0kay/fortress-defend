using Godot;

public partial class BuildingPlacement : Node3D
{
	[Export] public int GridCellsAcross { get; set; } = 15;

	private Camera3D _playerCamera;
	private WorldGrid _worldGrid;
	private PlacementValidator _validator;
	private BuildingDefinition _selectedBuilding;
	private Node3D _preview;
	private MeshInstance3D _gridVisual;

	public override void _Ready()
	{
		// GameMaker equivalent: Create event.
		_playerCamera = GetNode<Camera3D>("../Player/Camera3D");
		_worldGrid = GetNode<WorldGrid>("../WorldGrid");
		_validator = new PlacementValidator(_worldGrid);
	}

	public override void _Process(double delta)
	{
		// GameMaker equivalent: Step event. No work outside placement mode.
		if (_preview == null)
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = _playerCamera.ProjectRayOrigin(mousePosition);
		Vector3 rayDirection = _playerCamera.ProjectRayNormal(mousePosition);

		Plane ground = new(Vector3.Up, _worldGrid.GlobalPosition.Y);
		Vector3? hitPosition = ground.IntersectsRay(rayOrigin, rayDirection);

		if (hitPosition == null)
		{
			_preview.Hide();
			_gridVisual.Hide();
			return;
		}

		Vector3 position = _worldGrid.SnapFootprintCenter(
			hitPosition.Value,
			_selectedBuilding.FootprintCells
		);

		_preview.GlobalPosition = position;

		// Keep the visible lines aligned with world cells.
		Vector2I nearestCell = _worldGrid.WorldToCell(hitPosition.Value);
		_gridVisual.GlobalPosition = _worldGrid.CellToWorld(nearestCell);

	

		_preview.Show();
		_gridVisual.Show();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// GameMaker equivalent: a Step event synchronized with physics.
		if (_preview == null || !_preview.Visible)
			return;

		bool canPlace = _validator.CanPlace(
		GetWorld3D().DirectSpaceState,
		_preview.GlobalPosition,
		_selectedBuilding
	);

		UpdatePlacementFeedback(canPlace);
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
	Building preview = _selectedBuilding.Scene.Instantiate<Building>();

	// Must happen BEFORE AddChild, which triggers Building._Ready().
	preview.IsPreview = true;

	_preview = preview;
	AddChild(_preview);

	// The preview must not collide with anything.
	preview.CollisionLayer = 0;
	preview.CollisionMask = 0;

	CollisionShape3D collisionShape =
		preview.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");

	if (collisionShape != null)
		collisionShape.Disabled = true;

	MeshInstance3D mesh =
		preview.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

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

	private void UpdatePlacementFeedback(bool canPlace)
	{
		Color previewColor = canPlace
			? new Color(0.2f, 0.9f, 1.0f, 0.45f)
			: new Color(1.0f, 0.2f, 0.2f, 0.55f);

		Color gridColor = canPlace
			? new Color(0.5f, 0.85f, 1.0f, 0.55f)
			: new Color(1.0f, 0.25f, 0.25f, 0.65f);

		MeshInstance3D previewMesh =
			_preview.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

		if (previewMesh?.MaterialOverride is StandardMaterial3D previewMaterial)
			previewMaterial.AlbedoColor = previewColor;

		if (_gridVisual.MaterialOverride is StandardMaterial3D gridMaterial)
			gridMaterial.AlbedoColor = gridColor;
	}
}
