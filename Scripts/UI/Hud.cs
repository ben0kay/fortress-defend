using Godot;
using System;
using System.Collections.Generic;

public partial class Hud : CanvasLayer
{
	[Export] public BuildingCatalog Catalog { get; set; }

	private Player _player;
	private BuildingPlacement _buildingPlacement;
	private Label _carbonLabel;
	private PanelContainer _buildMenu;
	private HBoxContainer _buildingChoices;
	private HBoxContainer _categories;

	private readonly Dictionary<string, List<Button>> _choicesByCategory = new();
	private string _openCategory = "";

	public override void _Ready()
	{
		// GameMaker equivalent: Create event.
		_player = GetNode<Player>("../Player");
		_buildingPlacement = GetNode<BuildingPlacement>("../BuildingPlacement");

		_carbonLabel = GetNode<Label>("ResourceBar/ResourceCounts/CarbonLabel");
		_buildMenu = GetNode<PanelContainer>("BuildMenu");
		_buildingChoices = GetNode<HBoxContainer>("BuildMenu/MenuRows/BuildingChoices");
		_categories = GetNode<HBoxContainer>("BuildMenu/MenuRows/Categories");

		UpdateCarbonLabel(_player.Resources["carbon"]);
		_player.ResourceChanged += OnResourceChanged;

		CreateBuildingButtons();

		_buildMenu.Hide();
		_buildingChoices.Hide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_build_menu") && !@event.IsEcho())
			ToggleBuildMenu();
	}

	private void CreateBuildingButtons()
	{
		if (Catalog == null)
		{
			GD.PushWarning("Assign BuildingCatalog.tres to the HUD's Catalog field.");
			return;
		}

		// Sort once when the scene starts. No menu rebuilding every frame.
		List<BuildingDefinition> buildings = new();

		foreach (BuildingDefinition definition in Catalog.Buildings)
		{
			if (definition != null)
				buildings.Add(definition);
		}

		buildings.Sort((a, b) =>
		{
			int order = a.MenuOrder.CompareTo(b.MenuOrder);

			return order != 0
				? order
				: string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
		});

		foreach (BuildingDefinition definition in buildings)
		{
			string category = BuildingCatalog.GetMenuCategory(definition.Role);

			if (!_choicesByCategory.ContainsKey(category))
				CreateCategoryButton(category);

			CreateBuildingButton(definition, category);
		}
	}

	private void CreateCategoryButton(string category)
	{
		_choicesByCategory[category] = new List<Button>();

		Button button = new()
		{
			Text = category
		};

		_categories.AddChild(button);
		button.Pressed += () => ToggleCategory(category);
	}

	private void CreateBuildingButton(BuildingDefinition definition, string category)
	{
		Button button = new()
		{
			Text = definition.DisplayName,
			Visible = false
		};

		_buildingChoices.AddChild(button);
		_choicesByCategory[category].Add(button);

		button.Pressed += () => SelectBuilding(definition);
	}

	private void ToggleCategory(string category)
	{
		bool closingCurrentCategory = _openCategory == category;

		foreach (List<Button> buttons in _choicesByCategory.Values)
		{
			foreach (Button button in buttons)
				button.Hide();
		}

		_openCategory = closingCurrentCategory ? "" : category;

		if (_openCategory == "")
		{
			_buildingChoices.Hide();
			return;
		}

		foreach (Button button in _choicesByCategory[_openCategory])
			button.Show();

		_buildingChoices.Show();
	}

	private void SelectBuilding(BuildingDefinition definition)
	{
		if (definition.Scene == null)
		{
			GD.PushWarning($"Building '{definition.Key}' has no Scene assigned.");
			return;
		}

		_buildingPlacement.BeginPlacement(definition);
		_player.CurrentMode = Player.Mode.Placing;
		GD.Print("Placing: ", definition.DisplayName);
	}

	private void ToggleBuildMenu()
	{
		if (_player.CurrentMode == Player.Mode.Normal)
		{
			_player.CurrentMode = Player.Mode.BuildMenu;
			_buildMenu.Show();
			return;
		}

		_buildingPlacement.CancelPlacement();
		_player.CurrentMode = Player.Mode.Normal;
		_openCategory = "";

		foreach (List<Button> buttons in _choicesByCategory.Values)
		{
			foreach (Button button in buttons)
				button.Hide();
		}

		_buildingChoices.Hide();
		_buildMenu.Hide();
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
