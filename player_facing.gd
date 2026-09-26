extends Node3D

@onready var player_camera: Camera3D = $"../Camera3D"


func _process(_delta: float) -> void:
	# Find where the mouse points on a flat plane at the player's feet.
	var mouse_position := get_viewport().get_mouse_position()
	var ray_origin := player_camera.project_ray_origin(mouse_position)
	var ray_direction := player_camera.project_ray_normal(mouse_position)
	var ground_plane := Plane(Vector3.UP, global_position.y)

	var hit_position = ground_plane.intersects_ray(ray_origin, ray_direction)
	if hit_position == null:
		return

	# Turn the visible exosuit toward the mouse.
	var direction: Vector3 = hit_position - global_position
	direction.y = 0.0

	if direction.length_squared() < 0.01:
		return

	rotation.y = atan2(-direction.x, -direction.z)
