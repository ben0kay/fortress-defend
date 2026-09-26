using Godot;

[GlobalClass]
public partial class BuildingCost : Resource
{
	[Export] public string ResourceKey { get; set; } = "";
	[Export] public int Amount { get; set; } = 0;
}
