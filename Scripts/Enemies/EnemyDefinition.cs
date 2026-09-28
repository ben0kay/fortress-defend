using Godot;

[GlobalClass]
public partial class EnemyDefinition : Resource
{
	[ExportGroup("Identity")]
	[Export] public string Key { get; set; } = "";
	[Export] public string DisplayName { get; set; } = "";

	[ExportGroup("Vitals and Movement")]
	[Export] public int MaximumHealth { get; set; } = 5;
	[Export] public float MoveSpeed { get; set; } = 2.0f;

	[ExportGroup("Melee Attack")]
	[Export] public int AttackDamage { get; set; } = 1;
	[Export] public float AttackRange { get; set; } = 3.8f;
	[Export] public float AttackInterval { get; set; } = 1.0f;
}
