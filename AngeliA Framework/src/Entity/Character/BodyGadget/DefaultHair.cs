namespace AngeliA;

public sealed class DefaultHair : Hair {
	public static readonly int TYPE_ID = typeof(DefaultHair).AngeHash();
	public DefaultHair () => FillFromSheet(GetType().AngeName());
}
