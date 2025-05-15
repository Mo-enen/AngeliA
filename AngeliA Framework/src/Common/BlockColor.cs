namespace AngeliA;

/// <summary>
/// Element block that set the color tint of the overlapping level block
/// </summary>
[EntityAttribute.MapEditorGroup("System")]
public abstract class BlockColor : IMapItem {
	/// <summary>
	/// Target color tint
	/// </summary>
	public abstract Color32 Color { get; }
}
