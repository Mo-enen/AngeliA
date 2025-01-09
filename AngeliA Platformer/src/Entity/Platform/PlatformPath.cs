using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.MapEditorGroup(nameof(Platform))]
public sealed class PlatformPath : IMapItem {
	public static readonly int TYPE_ID = typeof(PlatformPath).AngeHash();
}
