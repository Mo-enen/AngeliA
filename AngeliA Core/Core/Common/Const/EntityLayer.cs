namespace AngeliA; 
public static class EntityLayer {
	public static readonly string[] LAYER_NAMES = new string[COUNT] {
		"Game", "Character", "Environment", "Bullet", "Item", "Decorate", "UI",
	};
	public const int GAME = 0;
	public const int CHARACTER = 1;
	public const int ENVIRONMENT = 2;
	public const int BULLET = 3;
	public const int ITEM = 4;
	public const int DECORATE = 5;
	public const int UI = 6;
	public const int COUNT = 7;
}