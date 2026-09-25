extends Node3D

@export var carbon_remaining: int = 40
@export var gather_interval_seconds: float = 0.25

@onready var gather_area: Area3D = $GatherArea

var player_in_range: Player = null
var gather_elapsed: float = 0.0


func _ready() -> void:
	# GameMaker equivalent: Create event.
	gather_area.body_entered.connect(_on_body_entered)
	gather_area.body_exited.connect(_on_body_exited)


func _physics_process(delta: float) -> void:
	# Holding middle mouse gathers one carbon every interval.
	if player_in_range == null or carbon_remaining <= 0:
		gather_elapsed = 0.0
		return

	if not Input.is_action_pressed("gather"):
		gather_elapsed = 0.0
		return

	gather_elapsed += delta

	while gather_elapsed >= gather_interval_seconds and carbon_remaining > 0:
		gather_elapsed -= gather_interval_seconds
		carbon_remaining -= 1
		player_in_range.add_resource("carbon", 1)

	if carbon_remaining == 0:
		$Visual.hide()


func _on_body_entered(body: Node3D) -> void:
	if body is Player:
		player_in_range = body


func _on_body_exited(body: Node3D) -> void:
	if body == player_in_range:
		player_in_range = null
		gather_elapsed = 0.0
