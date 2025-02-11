namespace AngeliA;

public sealed class DefaultFace : Face {
	public static readonly int TYPE_ID = typeof(DefaultFace).AngeHash();
	public DefaultFace () => FillFromSheet(GetType().AngeName());
}
