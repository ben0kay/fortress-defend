class_name PlayerGathering
extends Node

@onready var player: Player = get_parent() as Player

var nearby_deposits: Array[ResourceDeposit] = []
var current_deposit: ResourceDeposit = null
var gather_elapsed: float = 0.0


func _physics_process(delta: float) -> void:
	# GameMaker equivalent: a separate Step event for gathering.
	if player.mode != Player.Mode.NORMAL \
			or not Input.is_action_pressed("secondary_fire"):
		_reset_gathering()
		return

	var nearest_deposit := _find_nearest_deposit()

	if nearest_deposit == null:
		_reset_gathering()
		return

	if nearest_deposit != current_deposit:
		current_deposit = nearest_deposit
		gather_elapsed = 0.0

	gather_elapsed += delta

	while gather_elapsed >= current_deposit.gather_interval_seconds \
			and current_deposit.can_gather():
		gather_elapsed -= current_deposit.gather_interval_seconds
		current_deposit.gather_one(player)


func register_deposit(deposit: ResourceDeposit) -> void:
	if not nearby_deposits.has(deposit):
		nearby_deposits.append(deposit)


func unregister_deposit(deposit: ResourceDeposit) -> void:
	nearby_deposits.erase(deposit)

	if current_deposit == deposit:
		_reset_gathering()


func _find_nearest_deposit() -> ResourceDeposit:
	var nearest: ResourceDeposit = null
	var nearest_distance := INF

	# Check only deposits whose gathering areas contain the player.
	for deposit in nearby_deposits:
		if not deposit.can_gather():
			continue

		var distance := player.global_position.distance_squared_to(
			deposit.global_position
		)

		if distance < nearest_distance:
			nearest_distance = distance
			nearest = deposit

	return nearest


func _reset_gathering() -> void:
	current_deposit = null
	gather_elapsed = 0.0
