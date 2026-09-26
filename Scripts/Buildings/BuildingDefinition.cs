using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BuildingDefinition : Resource
{
	[ExportGroup("Identity")]
	[Export] public string Key { get; set; } = "";
	[Export] public string DisplayName { get; set; } = "";
	[Export] public string ShortDescription { get; set; } = "";
	[Export] public BuildingRole Role { get; set; } = BuildingRole.Support;
	[Export] public int MenuOrder { get; set; } = 0;

	[ExportGroup("Scene and Footprint")]
	[Export] public PackedScene Scene { get; set; }
	[Export] public Vector2I FootprintCells { get; set; } = Vector2I.One;

	[ExportGroup("Vitals and Construction")]
	[Export] public int MaximumHealth { get; set; } = 100;
	[Export] public float BuildTimeSeconds { get; set; } = 0.0f;

	[ExportGroup("Economy")]
	[Export] public Array<BuildingCost> Costs { get; set; } = new();
}
