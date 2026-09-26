using Godot;

public partial class Hud : CanvasLayer
{
	[Export] public PackedScene HeadquartersScene { get; set; }

	private Player _player;
	private BuildingPlacement _buildingPlacement;
	private Label _carbonLabel;
	private PanelContainer _buildMenu;
	private HBoxContainer _buildingChoices;
	private Button _baseButton;
	private Button _headquartersButton;

	public override void _Ready()
	{
		// GameMaker equivalent: Create event.
		_player = GetNode<Player>("../Player");
		_buildingPlacement = GetNode<BuildingPlacement>("../BuildingPlacement");

		_carbonLabel = GetNode<Label>("ResourceBar/ResourceCounts/CarbonLabel");
		_buildMenu = GetNode<PanelContainer>("BuildMenu");
		_buildingChoices = GetNode<HBoxContainer>("BuildMenu/MenuRows/BuildingChoices");
		_baseButton = GetNode<Button>("BuildMenu/MenuRows/Categories/BaseButton");
		_headquartersButton = GetNode<Button>(
            "BuildMenu/MenuRows/BuildingChoices/HeadquartersButton"
		);

		UpdateCarbonLabel(_player.Resources["carbon"]);

		_player.ResourceChanged += OnResourceChanged;
		_baseButton.Pressed += OnBaseButtonPressed;
		_headquartersButton.Pressed += OnHeadquartersButtonPressed;

		_buildMenu.Hide();
		_buildingChoices.Hide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_build_menu") && !@event.IsEcho())
			ToggleBuildMenu();
	}

	private void ToggleBuildMenu()
	{
		if (_player.CurrentMode == Player.Mode.Normal)
		{
			_player.CurrentMode = Player.Mode.BuildMenu;
			_buildMenu.Show();
		}
		else
		{
			_buildingPlacement.CancelPlacement();
			_player.CurrentMode = Player.Mode.Normal;
			_buildingChoices.Hide();
			_buildMenu.Hide();
		}
	}

	private void OnBaseButtonPressed()
	{
		// Press Base again to collapse its building choices.
		_buildingChoices.Visible = !_buildingChoices.Visible;
	}

	private void OnHeadquartersButtonPressed()
	{
		if (HeadquartersScene == null)
		{
			GD.PushWarning("Assign Headquarters Scene on the HUD node.");
			return;
		}

		_buildingPlacement.BeginPlacement(HeadquartersScene);
		_player.CurrentMode = Player.Mode.Placing;
		GD.Print("Placing: Headquarters");
	}

	private void OnResourceChanged(string resourceKey, int newAmount)
	{
		if (resourceKey == "carbon")
			UpdateCarbonLabel(newAmount);
	}

	private void UpdateCarbonLabel(int amount)
	{
		_carbonLabel.Text = "Carbon: " + amount;
	}
}
