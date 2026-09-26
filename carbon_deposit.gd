class_name ResourceDeposit
extends Node3D

@export var resource_key: String = "carbon"
@export var resource_remaining: int = 40
@export_range(0.05, 10.0, 0.05) var gather_interval_seconds: float = 0.25

@onready var gather_area: Area3D = $GatherArea


func _ready() -> void:
	# Tell players when this deposit enters or leaves their gathering range.
	gather_area.body_entered.connect(_on_body_entered)
	gather_area.body_exited.connect(_on_body_exited)


func can_gather() -> bool:
	return resource_remaining > 0


func gather_one(player: Player) -> void:
	# The deposit controls its contents; the player controls when to ask.
	if not can_gather():
		return

	resource_remaining -= 1
	player.add_resource(resource_key, 1)

	if resource_remaining == 0:
		$Visual.hide()


func _on_body_entered(body: Node3D) -> void:
	var player := body as Player
	if player != null:
		player.gathering.register_deposit(self)


func _on_body_exited(body: Node3D) -> void:
	var player := body as Player
	if player != null:
		player.gathering.unregister_deposit(self)
