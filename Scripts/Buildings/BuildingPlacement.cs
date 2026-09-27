using Godot;

public partial class BuildingPlacement : Node3D
{
	[Export] public int GridCellsAcross { get; set; } = 15;

	private Camera3D _playerCamera;
	private WorldGrid _worldGrid;
	private ResourceInventory _inventory;
	private PlacementValidator _validator;
	private BuildingDefinition _selectedBuilding;
	private Node3D _preview;
	private MeshInstance3D _gridVisual;
	private bool _placementClickPending;

	public override void _Ready()
	{
		_playerCamera = GetNode<Camera3D>("../Player/Camera3D");
		_worldGrid = GetNode<WorldGrid>("../WorldGrid");
		_inventory = GetNode<ResourceInventory>("../ResourceInventory");
		_validator = new PlacementValidator(_worldGrid, _inventory);
	}

	public override void _Process(double delta)
	{
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

		Vector2I nearestCell = _worldGrid.WorldToCell(hitPosition.Value);
		_gridVisual.GlobalPosition = _worldGrid.CellToWorld(nearestCell);

		_preview.Show();
		_gridVisual.Show();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_preview == null || !_preview.Visible)
		{
			_placementClickPending = false;
			return;
		}

		bool canPlace = _validator.CanPlace(
			GetWorld3D().DirectSpaceState,
			_preview.GlobalPosition,
			_selectedBuilding
		);

		UpdatePlacementFeedback(canPlace);

		if (!_placementClickPending)
			return;

		_placementClickPending = false;

		if (canPlace)
			PlaceBuilding(_preview.GlobalPosition);
	}

	private void PlaceBuilding(Vector3 worldPosition)
	{
		// Check again at the moment of purchase.
		if (!_validator.CanPlace(
			GetWorld3D().DirectSpaceState,
			worldPosition,
			_selectedBuilding))
			return;

		if (!_inventory.TryPay(_selectedBuilding))
			return;

		Building building = _selectedBuilding.Scene.Instantiate<Building>();
		building.Definition = _selectedBuilding;
		building.Position = GetParent<Node3D>().ToLocal(worldPosition);

		GetParent().AddChild(building);

		if (!building.HasReservedCells)
		{
			building.QueueFree();
			_inventory.Refund(_selectedBuilding);
			GD.PushError(
				$"Could not place {_selectedBuilding.DisplayName}; cost refunded."
			);
			return;
		}

		GD.Print("Built: ", _selectedBuilding.DisplayName);
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
		preview.IsPreview = true;

		_preview = preview;
		AddChild(_preview);

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
		int cells = Mathf.Max(
			GridCellsAcross,
			Mathf.Max(
				_selectedBuilding.FootprintCells.X,
				_selectedBuilding.FootprintCells.Y
			) + 4
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

			lines.SurfaceAddVertex(
				new Vector3(coordinate, 0.04f, -halfWidth)
			);
			lines.SurfaceAddVertex(
				new Vector3(coordinate, 0.04f, halfWidth)
			);

			lines.SurfaceAddVertex(
				new Vector3(-halfWidth, 0.04f, coordinate)
			);
			lines.SurfaceAddVertex(
				new Vector3(halfWidth, 0.04f, coordinate)
			);
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
		_placementClickPending = false;
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

		if (previewMesh?.MaterialOverride is
			StandardMaterial3D previewMaterial)
			previewMaterial.AlbedoColor = previewColor;

		if (_gridVisual.MaterialOverride is StandardMaterial3D gridMaterial)
			gridMaterial.AlbedoColor = gridColor;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_preview == null)
			return;

		if (@event is InputEventMouseButton mouseButton &&
			mouseButton.ButtonIndex == MouseButton.Left &&
			mouseButton.Pressed)
		{
			_placementClickPending = true;
		}
	}
}
