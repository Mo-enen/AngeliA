namespace AngeliA;

public sealed class DefaultFootSuit : FootCloth {
	public static readonly int TYPE_ID = typeof(DefaultFootSuit).AngeHash();
	public DefaultFootSuit () => FillFromSheet(GetType().AngeName());
}
