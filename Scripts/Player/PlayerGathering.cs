using Godot;
using System.Collections.Generic;

public partial class PlayerGathering : Node
{
	private Player _player;
	private readonly List<ResourceDeposit> _nearbyDeposits = new();

	private ResourceDeposit _currentDeposit;
	private float _gatherElapsed = 0.0f;

	public override void _Ready()
	{
		_player = GetParent<Player>();
	}

	public override void _PhysicsProcess(double delta)
	{
		// GameMaker equivalent: a separate Step event for gathering.
		if (_player.CurrentMode != Player.Mode.Normal ||
			!Input.IsActionPressed("secondary_fire"))
		{
			ResetGathering();
			return;
		}

		ResourceDeposit nearestDeposit = FindNearestDeposit();

		if (nearestDeposit == null)
		{
			ResetGathering();
			return;
		}

		if (nearestDeposit != _currentDeposit)
		{
			_currentDeposit = nearestDeposit;
			_gatherElapsed = 0.0f;
		}

		_gatherElapsed += (float)delta;

		while (_gatherElapsed >= _currentDeposit.GatherIntervalSeconds &&
			   _currentDeposit.CanGather())
		{
			_gatherElapsed -= _currentDeposit.GatherIntervalSeconds;
			_currentDeposit.GatherOne(_player);
		}
	}

	public void RegisterDeposit(ResourceDeposit deposit)
	{
		if (!_nearbyDeposits.Contains(deposit))
			_nearbyDeposits.Add(deposit);
	}

	public void UnregisterDeposit(ResourceDeposit deposit)
	{
		_nearbyDeposits.Remove(deposit);

		if (_currentDeposit == deposit)
			ResetGathering();
	}

	private ResourceDeposit FindNearestDeposit()
	{
		ResourceDeposit nearest = null;
		float nearestDistance = float.PositiveInfinity;

		// Check only deposits whose gathering areas contain the player.
		foreach (ResourceDeposit deposit in _nearbyDeposits)
		{
			if (!deposit.CanGather())
				continue;

			float distance = _player.GlobalPosition.DistanceSquaredTo(
				deposit.GlobalPosition
			);

			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = deposit;
			}
		}

		return nearest;
	}

	private void ResetGathering()
	{
		_currentDeposit = null;
		_gatherElapsed = 0.0f;
	}
}
