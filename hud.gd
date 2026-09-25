extends CanvasLayer

@onready var player: Player = $"../Player"
@onready var carbon_label: Label = $ResourceBar/ResourceCounts/CarbonLabel
@onready var build_menu: PanelContainer = $BuildMenu


func _ready() -> void:
	# GameMaker equivalent: Create event.
	_update_carbon_label(player.resources["carbon"])
	player.resource_changed.connect(_on_resource_changed)
	build_menu.hide()


func _unhandled_input(event: InputEvent) -> void:
	# Control switches between normal play and the build menu.
	if event.is_action_pressed("toggle_build_menu") and not event.is_echo():
		_toggle_build_menu()


func _toggle_build_menu() -> void:
	if player.mode == Player.Mode.NORMAL:
		player.mode = Player.Mode.BUILD_MENU
		build_menu.show()
	else:
		player.mode = Player.Mode.NORMAL
		build_menu.hide()


func _on_resource_changed(resource_key: String, new_amount: int) -> void:
	if resource_key == "carbon":
		_update_carbon_label(new_amount)


func _update_carbon_label(amount: int) -> void:
	carbon_label.text = "Carbon: " + str(amount)
