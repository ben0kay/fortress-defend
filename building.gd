class_name Building
extends StaticBody3D

signal health_changed(current_health: int, maximum_health: int)
signal destroyed(building: Building)

@export var maximum_health: int = 100

var current_health: int = 0


func _ready() -> void:
	# GameMaker equivalent: Create event.
	current_health = maximum_health


func take_damage(amount: int) -> void:
	if amount <= 0 or current_health <= 0:
		return

	current_health = max(current_health - amount, 0)
	health_changed.emit(current_health, maximum_health)

	if current_health == 0:
		destroyed.emit(self)
