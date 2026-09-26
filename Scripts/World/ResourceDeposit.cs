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

		// Tell players when this deposit enters or leaves gathering range.
		_gatherArea.BodyEntered += OnBodyEntered;
		_gatherArea.BodyExited += OnBodyExited;
	}

	public bool CanGather()
	{
		return ResourceRemaining > 0;
	}

	public void GatherOne(Player player)
	{
		// The deposit controls its contents; the player controls when to ask.
		if (!CanGather())
			return;

		ResourceRemaining -= 1;
		player.AddResource(ResourceKey, 1);

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
