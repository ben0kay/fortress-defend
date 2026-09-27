using Godot;

public partial class BuildingPlacement : Node3D
{
	[Export] public float GridSize { get; set; } = 2.0f;
	[Export] public int GridCellsAcross { get; set; } = 15;

	private Camera3D _playerCamera;
	private BuildingDefinition _selectedBuilding;
	private Node3D _preview;
	private MeshInstance3D _grid;

	public override void _Ready()
	{
		// GameMaker equivalent: Create event.
		_playerCamera = GetNode<Camera3D>("../Player/Camera3D");
	}

	public override void _Process(double delta)
	{
		// GameMaker equivalent: Step event. No placement work outside placement mode.
		if (_preview == null)
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = _playerCamera.ProjectRayOrigin(mousePosition);
		Vector3 rayDirection = _playerCamera.ProjectRayNormal(mousePosition);

		// The sandbox floor surface is at Y = 0.
		Plane floorPlane = new(Vector3.Up, 0.0f);
		Vector3? hitPosition = floorPlane.IntersectsRay(rayOrigin, rayDirection);

		if (hitPosition == null)
		{
			_preview.Hide();
			_grid.Hide();
			return;
		}

		Vector3 position = hitPosition.Value;
		position.X = Mathf.Round(position.X / GridSize) * GridSize;
		position.Y = 0.0f;
		position.Z = Mathf.Round(position.Z / GridSize) * GridSize;

		_preview.GlobalPosition = position;
		_grid.GlobalPosition = position;

		_preview.Show();
		_grid.Show();
	}

	public void BeginPlacement(BuildingDefinition definition)
	{
		CancelPlacement();

		if (definition == null || definition.Scene == null)
			return;

		_selectedBuilding = definition;
		CreatePreview();
		CreateGrid();
	}

	private void CreatePreview()
	{
		// Instantiate once when a building is selected.
		_preview = _selectedBuilding.Scene.Instantiate<Node3D>();
		AddChild(_preview);

		// The preview is visual only and cannot block the player.
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

	private void CreateGrid()
	{
		// Build the line mesh once per selection; moving it does not rebuild it.
		int cells = Mathf.Max(
			GridCellsAcross,
			Mathf.Max(_selectedBuilding.FootprintCells.X,
					  _selectedBuilding.FootprintCells.Y) + 4
		);

		if (cells % 2 == 0)
			cells++;

		float halfWidth = cells * GridSize * 0.5f;
		ImmediateMesh lines = new();

		lines.SurfaceBegin(Mesh.PrimitiveType.Lines);

		for (int line = 0; line <= cells; line++)
		{
			float coordinate = -halfWidth + line * GridSize;

			// Line running along Z.
			lines.SurfaceAddVertex(new Vector3(coordinate, 0.04f, -halfWidth));
			lines.SurfaceAddVertex(new Vector3(coordinate, 0.04f, halfWidth));

			// Line running along X.
			lines.SurfaceAddVertex(new Vector3(-halfWidth, 0.04f, coordinate));
			lines.SurfaceAddVertex(new Vector3(halfWidth, 0.04f, coordinate));
		}

		lines.SurfaceEnd();

		_grid = new MeshInstance3D
		{
			Mesh = lines,
			MaterialOverride = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				AlbedoColor = new Color(0.5f, 0.85f, 1.0f, 0.55f)
			}
		};

		AddChild(_grid);
	}

	public void CancelPlacement()
	{
		_selectedBuilding = null;

		if (_preview != null)
		{
			_preview.QueueFree();
			_preview = null;
		}

		if (_grid != null)
		{
			_grid.QueueFree();
			_grid = null;
		}
	}
}
