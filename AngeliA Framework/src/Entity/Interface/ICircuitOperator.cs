using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Interface that makes an entity behave like operator in circuit system
/// </summary>
public interface ICircuitOperator {

	private static readonly HashSet<int> OperatorSet = [];

	[OnGameInitialize]
	internal static void OnGameInitialize () {
		OperatorSet.Clear();
		foreach (var type in typeof(ICircuitOperator).AllClassImplemented()) {
			OperatorSet.Add(type.AngeHash());
		}
	}

	/// <summary>
	/// True if the given type of entity is a circuit operator
	/// </summary>
	public static bool IsOperator (int typeID) => OperatorSet.Contains(typeID);

	/// <summary>
	/// This function is called when the operator get triggered by the system
	/// </summary>
	public void OnTriggeredByCircuit ();

}
