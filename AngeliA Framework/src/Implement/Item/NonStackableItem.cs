namespace AngeliA;

public abstract class NonStackableItem : Item {
	public sealed override int MaxStackCount => 1;
}
