using Godot;
using System.Collections.Generic;

public partial class ResourceInventory : Node
{
	[Signal]
	public delegate void ResourceChangedEventHandler(
		string resourceKey,
		int newAmount
	);

	private readonly Dictionary<string, int> _amounts = new();

	public int GetAmount(string resourceKey)
	{
		return _amounts.TryGetValue(resourceKey, out int amount)
			? amount
			: 0;
	}

	public bool Add(string resourceKey, int amount)
	{
		if (string.IsNullOrWhiteSpace(resourceKey) || amount <= 0)
			return false;

		int newAmount = GetAmount(resourceKey) + amount;
		_amounts[resourceKey] = newAmount;
		EmitSignal(SignalName.ResourceChanged, resourceKey, newAmount);
		return true;
	}

	public bool CanAfford(BuildingDefinition definition)
	{
		if (definition == null || definition.Costs == null)
			return false;

		Dictionary<string, int> required = new();

		foreach (BuildingCost cost in definition.Costs)
		{
			if (cost == null ||
				string.IsNullOrWhiteSpace(cost.ResourceKey) ||
				cost.Amount < 0)
				return false;

			required.TryGetValue(cost.ResourceKey, out int subtotal);
			required[cost.ResourceKey] = subtotal + cost.Amount;
		}

		foreach (KeyValuePair<string, int> entry in required)
		{
			if (GetAmount(entry.Key) < entry.Value)
				return false;
		}

		return true;
	}

	public bool TryPay(BuildingDefinition definition)
	{
		if (!CanAfford(definition))
			return false;

		foreach (BuildingCost cost in definition.Costs)
		{
			if (cost.Amount == 0)
				continue;

			int remaining = GetAmount(cost.ResourceKey) - cost.Amount;
			_amounts[cost.ResourceKey] = remaining;
			EmitSignal(SignalName.ResourceChanged, cost.ResourceKey, remaining);
		}

		return true;
	}

	public void Refund(BuildingDefinition definition)
	{
		if (definition == null)
			return;

		foreach (BuildingCost cost in definition.Costs)
		{
			if (cost != null && cost.Amount > 0)
				Add(cost.ResourceKey, cost.Amount);
		}
	}
}
