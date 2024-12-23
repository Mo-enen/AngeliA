namespace AngeliA;

public sealed class DefaultBodySuit : BodyCloth {
	public static readonly int TYPE_ID = typeof(DefaultBodySuit).AngeHash();
	public DefaultBodySuit () => FillFromSheet(GetType().AngeName());
}

public sealed class DefaultFootSuit : FootCloth {
	public static readonly int TYPE_ID = typeof(DefaultFootSuit).AngeHash();
	public DefaultFootSuit () => FillFromSheet(GetType().AngeName());
}

public sealed class DefaultHipSuit : HipCloth {
	public static readonly int TYPE_ID = typeof(DefaultHipSuit).AngeHash();
	public DefaultHipSuit () => FillFromSheet(GetType().AngeName());
}
