using Godot;

public partial class PlayerFacing : Node3D
{
	private Camera3D _playerCamera;

	public override void _Ready()
	{
		_playerCamera = GetNode<Camera3D>("../Camera3D");
	}

	public override void _Process(double delta)
	{
		// Find where the mouse points on a flat plane at the player's feet.
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = _playerCamera.ProjectRayOrigin(mousePosition);
		Vector3 rayDirection = _playerCamera.ProjectRayNormal(mousePosition);
		Plane groundPlane = new(Vector3.Up, GlobalPosition.Y);

		Vector3? hitPosition = groundPlane.IntersectsRay(rayOrigin, rayDirection);

		if (hitPosition == null)
			return;

		// Turn only the visible exosuit toward the mouse.
		Vector3 direction = hitPosition.Value - GlobalPosition;
		direction.Y = 0.0f;

		if (direction.LengthSquared() < 0.01f)
			return;

		Rotation = new Vector3(
			Rotation.X,
			Mathf.Atan2(-direction.X, -direction.Z),
			Rotation.Z
		);
	}
}
