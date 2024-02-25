namespace AngeliA; 
public static class RenderLayer {

	public const int WALLPAPER = 0;
	public const int BEHIND = 1;
	public const int SHADOW = 2;
	public const int DEFAULT = 3;
	public const int COLOR = 4;
	public const int MULT = 5;
	public const int ADD = 6;
	public const int UI = 7;

	public const int COUNT = 8;


	public static readonly string[] NAMES = new string[COUNT] {
		"Wallpaper", "Behind", "Shadow", "Default", "Color", "Mult", "Add", "UI",
	};

	public static readonly int[] CAPACITY = {
		WALLPAPER_CAPACITY,
		BEHIND_CAPACITY,
		SHADOW_CAPACITY,
		DEFAULT_CAPACITY,
		COLOR_CAPACITY,
		MULT_CAPACITY,
		ADD_CAPACITY,
		UI_CAPACITY,
		TOP_UI_CAPACITY,
	};
	public const int WALLPAPER_CAPACITY = 256;
	public const int BEHIND_CAPACITY = 8192;
	public const int SHADOW_CAPACITY = 4096;
	public const int DEFAULT_CAPACITY = 8192;
	public const int COLOR_CAPACITY = 256;
	public const int MULT_CAPACITY = 128;
	public const int ADD_CAPACITY = 128;
	public const int UI_CAPACITY = 4096;
	public const int TOP_UI_CAPACITY = 256;

}