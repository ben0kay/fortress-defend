extends CharacterBody3D

@export var move_speed: float = 5.0


func _physics_process(delta: float) -> void:
	# GameMaker equivalent: Step event for movement and collisions.
	_apply_movement()
	_apply_gravity(delta)
	move_and_slide()


func _apply_movement() -> void:
	# Read our four actions as one 2D direction.
	var input_direction := Input.get_vector(
		"move_left",
		"move_right",
		"move_up",
		"move_down"
	)

	# The player walks across the 3D floor: X is left/right, Z is up/down.
	velocity.x = input_direction.x * move_speed
	velocity.z = input_direction.y * move_speed


func _apply_gravity(delta: float) -> void:
	if is_on_floor():
		velocity.y = 0.0
	else:
		velocity += get_gravity() * delta
