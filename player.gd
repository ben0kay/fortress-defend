extends CharacterBody3D

@export var move_speed: float = 5.0


func _physics_process(delta: float) -> void:
	var input_direction := Input.get_vector(
		"ui_left",
		"ui_right",
		"ui_up",
		"ui_down"
	)

	velocity.x = input_direction.x * move_speed
	velocity.z = input_direction.y * move_speed

	if not is_on_floor():
		velocity += get_gravity() * delta
	else:
		velocity.y = 0.0

	move_and_slide()
