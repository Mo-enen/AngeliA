using System.Collections.Generic;

namespace AngeliA;

public interface ICircuitOperator {
	private static readonly HashSet<int> OperatorSet = [];
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		OperatorSet.Clear();
		foreach (var type in typeof(ICircuitOperator).AllClassImplemented()) {
			OperatorSet.Add(type.AngeHash());
		}
	}
	public static bool IsOperator (int typeID) => OperatorSet.Contains(typeID);
	public void TriggerCircuit ();
}
