namespace AngeliA;

/// <summary>
/// A type of item that do not stack-up inside inventory panel. The MaxStackCount is always 1.
/// </summary>
public abstract class NonStackableItem : Item {
	public sealed override int MaxStackCount => 1;
}
