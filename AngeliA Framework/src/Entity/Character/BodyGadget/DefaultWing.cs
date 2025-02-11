namespace AngeliA;

public sealed class DefaultWing : Wing {
	public static readonly int TYPE_ID = typeof(DefaultWing).AngeHash();
	protected override int Scale => 600;
}
