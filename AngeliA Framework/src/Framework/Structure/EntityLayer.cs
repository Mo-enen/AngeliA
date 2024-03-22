namespace AngeliA;

public static class EntityLayer {
	public static readonly string[] LAYER_NAMES = new string[COUNT] {
		"UI","Game", "Character", "Environment", "Bullet", "Item", "Decorate",
	};
	public const int UI = 0;
	public const int GAME = 1;
	public const int CHARACTER = 2;
	public const int ENVIRONMENT = 3;
	public const int BULLET = 4;
	public const int ITEM = 5;
	public const int DECORATE = 6;
	public const int COUNT = 7;
}