namespace AngeliA;

/// <summary>
/// Layer for rendering
/// </summary>
public static class RenderLayer {

	/// <summary>
	/// Most behind rendering layer, only overlap on top of sky color
	/// </summary>
	public const int WALLPAPER = 0;
	/// <summary>
	/// Behind layer of level blocks
	/// </summary>
	public const int BEHIND = 1;
	/// <summary>
	/// Hold shadow of level blocks and environment entities
	/// </summary>
	public const int SHADOW = 2;
	public const int DEFAULT = 3;
	/// <summary>
	/// Render cells with pure color, ignore sprite content
	/// </summary>
	public const int COLOR = 4;
	/// <summary>
	/// Render cells with multiply shader
	/// </summary>
	public const int MULT = 5;
	/// <summary>
	/// Render cells with additive shader
	/// </summary>
	public const int ADD = 6;
	public const int UI = 7;

	public const int COUNT = 8;

	public static readonly string[] NAMES = [
		"Wallpaper", "Behind", "Shadow", "Default", "Color", "Mult", "Add", "UI",
	];
	public static readonly int[] DEFAULT_CAPACITY = [
		1024, 16384, 8192, 65536, 512, 256, 256, 8192,
	];

}