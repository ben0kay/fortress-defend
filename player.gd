class_name Player
extends CharacterBody3D

enum Mode { NORMAL, BUILD_MENU, PLACING }

signal resource_changed(resource_key: String, new_amount: int)

@export var move_speed: float = 5.0

var mode: Mode = Mode.NORMAL

var resources: Dictionary = {
	"carbon": 0
}

var nearby_deposits: Array[ResourceDeposit] = []
var current_deposit: ResourceDeposit = null
var gather_elapsed: float = 0.0


func _physics_process(delta: float) -> void:
	# GameMaker equivalent: Step event.
	_apply_movement()
	_apply_gravity(delta)
	move_and_slide()
	_update_gathering(delta)


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


func register_deposit(deposit: ResourceDeposit) -> void:
	if not nearby_deposits.has(deposit):
		nearby_deposits.append(deposit)


func unregister_deposit(deposit: ResourceDeposit) -> void:
	nearby_deposits.erase(deposit)

	if current_deposit == deposit:
		_reset_gathering()


func _update_gathering(delta: float) -> void:
	# Movement still works in build mode, but secondary fire does not gather.
	if mode != Mode.NORMAL or not Input.is_action_pressed("secondary_fire"):
		_reset_gathering()
		return

	var nearest_deposit := _find_nearest_deposit()

	if nearest_deposit == null:
		_reset_gathering()
		return

	# Switching deposits starts a fresh gathering interval.
	if nearest_deposit != current_deposit:
		current_deposit = nearest_deposit
		gather_elapsed = 0.0

	gather_elapsed += delta

	while gather_elapsed >= current_deposit.gather_interval_seconds \
			and current_deposit.can_gather():
		gather_elapsed -= current_deposit.gather_interval_seconds
		current_deposit.gather_one(self)


func _find_nearest_deposit() -> ResourceDeposit:
	var nearest: ResourceDeposit = null
	var nearest_distance := INF

	# Only examine deposits whose gathering areas we are inside.
	for deposit in nearby_deposits:
		if not deposit.can_gather():
			continue

		var distance := global_position.distance_squared_to(deposit.global_position)

		if distance < nearest_distance:
			nearest_distance = distance
			nearest = deposit

	return nearest


func _reset_gathering() -> void:
	current_deposit = null
	gather_elapsed = 0.0


func add_resource(resource_key: String, amount: int) -> void:
	if amount <= 0:
		return

	var current_amount: int = resources.get(resource_key, 0)
	var new_amount: int = current_amount + amount

	resources[resource_key] = new_amount
	resource_changed.emit(resource_key, new_amount)

	# Temporary feedback alongside the HUD.
	print(resource_key, ": ", new_amount)
