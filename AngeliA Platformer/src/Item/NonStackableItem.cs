using AngeliA;namespace AngeliA.Platformer;

public abstract class NonStackableItem : Item {
	public sealed override int MaxStackCount => 1;
}
