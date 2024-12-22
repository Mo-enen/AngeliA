namespace AngeliA.Platformer;

public abstract class ButtonOperator {
	public int OperatorID { get; init; }
	public ButtonOperator () => OperatorID = GetType().AngeHash();
	public abstract void Operate (IBlockSquad squad, Int3 operatorUnitPos, Int3 buttonUnitPos);
}
