namespace AngeliA;

public sealed class DefaultHipSuit : HipCloth {
	public static readonly int TYPE_ID = typeof(DefaultHipSuit).AngeHash();
	public DefaultHipSuit () => FillFromSheet(GetType().AngeName());
}
