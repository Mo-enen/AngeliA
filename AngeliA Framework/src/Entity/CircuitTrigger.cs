using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CircuitTrigger : Entity, IBlockEntity {

	// VAR
	private static readonly HashSet<int> TriggerSet = [];

	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		TriggerSet.Clear();
		foreach (var type in typeof(CircuitTrigger).AllChildClass()) {
			TriggerSet.Add(type.AngeHash());
		}
	}

	public virtual void TriggerCircuit () => CircuitSystem.TriggerCircuit((X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ, maxUnitDistance: Const.MAP);

	// API
	public static bool IsCircuitTrigger (int typeID) => TriggerSet.Contains(typeID);

}
