using Godot;

public partial class BuildingPlacement : Node3D
{
	[Export] public float GridSize { get; set; } = 2.0f;

	private Camera3D _playerCamera;
	private Node3D _preview;

	public override void _Ready()
	{
		// The camera stays with the player; the preview lives in the world.
		_playerCamera = GetNode<Camera3D>("../Player/Camera3D");
	}

	public override void _Process(double delta)
	{
		if (_preview == null)
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = _playerCamera.ProjectRayOrigin(mousePosition);
		Vector3 rayDirection = _playerCamera.ProjectRayNormal(mousePosition);

		// Our current sandbox floor is at Y = 0.
		Plane floorPlane = new(Vector3.Up, 0.0f);
		Vector3? hitPosition = floorPlane.IntersectsRay(rayOrigin, rayDirection);

		if (hitPosition == null)
		{
			_preview.Hide();
			return;
		}

		Vector3 position = hitPosition.Value;
		position.X = Mathf.Round(position.X / GridSize) * GridSize;
		position.Y = 0.0f;
		position.Z = Mathf.Round(position.Z / GridSize) * GridSize;

		_preview.GlobalPosition = position;
		_preview.Show();
	}

	public void BeginPlacement(PackedScene buildingScene)
	{
		CancelPlacement();

		if (buildingScene == null)
			return;

		// Make one reusable preview instance. We do not create one every frame.
		_preview = buildingScene.Instantiate<Node3D>();
		AddChild(_preview);

		// The preview must not act as a solid building.
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
			// Cyan and partly transparent so it reads as a preview.
			mesh.MaterialOverride = new StandardMaterial3D
			{
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				AlbedoColor = new Color(0.2f, 0.9f, 1.0f, 0.45f)
			};
		}
	}

	public void CancelPlacement()
	{
		if (_preview == null)
			return;

		_preview.QueueFree();
		_preview = null;
	}
}
