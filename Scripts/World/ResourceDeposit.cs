using Godot;

public partial class ResourceDeposit : Node3D
{
	[Export] public string ResourceKey { get; set; } = "carbon";
	[Export] public int ResourceRemaining { get; set; } = 40;

	[Export(PropertyHint.Range, "0.05,10.0,0.05")]
	public float GatherIntervalSeconds { get; set; } = 0.25f;

	private Area3D _gatherArea;
	private Node3D _visual;

	public override void _Ready()
	{
		_gatherArea = GetNode<Area3D>("GatherArea");
		_visual = GetNode<Node3D>("Visual");

		_gatherArea.BodyEntered += OnBodyEntered;
		_gatherArea.BodyExited += OnBodyExited;
	}

	public bool CanGather()
	{
		return ResourceRemaining > 0;
	}

	public void GatherOne(ResourceInventory inventory)
	{
		if (!CanGather() || !inventory.Add(ResourceKey, 1))
			return;

		ResourceRemaining--;

		if (ResourceRemaining == 0)
			_visual.Hide();
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player player)
			player.Gathering.RegisterDeposit(this);
	}

	private void OnBodyExited(Node3D body)
	{
		if (body is Player player)
			player.Gathering.UnregisterDeposit(this);
	}
}
