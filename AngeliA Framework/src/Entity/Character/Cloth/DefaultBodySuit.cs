namespace AngeliA;

public sealed class DefaultBodySuit : BodyCloth {
	public static readonly int TYPE_ID = typeof(DefaultBodySuit).AngeHash();
	public DefaultBodySuit () => FillFromSheet(GetType().AngeName());
}
