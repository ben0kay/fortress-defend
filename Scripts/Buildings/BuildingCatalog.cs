using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuildingCatalog : Resource
{
	[Export] public Array<BuildingDefinition> Buildings { get; set; } = new();

	public BuildingDefinition FindByKey(string key)
	{
		foreach (BuildingDefinition building in Buildings)
		{
			if (building != null && building.Key == key)
				return building;
		}

		return null;
	}

	public static string GetMenuCategory(BuildingRole role)
	{
		return role switch
		{
			BuildingRole.Headquarters => "Base",
			BuildingRole.Wall or BuildingRole.Gate => "Defense",
			BuildingRole.Tower => "Towers",
			BuildingRole.Extractor => "Extraction",
			BuildingRole.Storage => "Storage",
			BuildingRole.Generator or BuildingRole.PowerNetwork => "Power",
			BuildingRole.Production => "Production",
			BuildingRole.Foundation => "Foundation",
			_ => "Support"
		};
	}
}
