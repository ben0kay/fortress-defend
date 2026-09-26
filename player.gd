class_name Player
extends CharacterBody3D

enum Mode { NORMAL, BUILD_MENU, PLACING }

signal resource_changed(resource_key: String, new_amount: int)

@export var move_speed: float = 5.0

@onready var gathering: PlayerGathering = $Gathering

var mode: Mode = Mode.NORMAL

var resources: Dictionary = {
	"carbon": 0
}


func _physics_process(delta: float) -> void:
	# GameMaker equivalent: Step event for movement and collisions.
	_apply_movement()
	_apply_gravity(delta)
	move_and_slide()


func _apply_movement() -> void:
	var input_direction := Input.get_vector(
		"move_left",
		"move_right",
		"move_up",
		"move_down"
	)

	velocity.x = input_direction.x * move_speed
	velocity.z = input_direction.y * move_speed


func _apply_gravity(delta: float) -> void:
	if is_on_floor():
		velocity.y = 0.0
	else:
		velocity += get_gravity() * delta


func add_resource(resource_key: String, amount: int) -> void:
	if amount <= 0:
		return

	var current_amount: int = resources.get(resource_key, 0)
	var new_amount: int = current_amount + amount

	resources[resource_key] = new_amount
	resource_changed.emit(resource_key, new_amount)

	# Temporary feedback alongside the HUD.
	print(resource_key, ": ", new_amount)
