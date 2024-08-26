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
	public static readonly int[] CAPACITY = new int[COUNT] {
		256, 8192, 4096, 16384, 256, 128, 128, 8192,
	};



}