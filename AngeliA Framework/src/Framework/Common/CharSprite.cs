namespace AngeliA;

public class CharSprite {
	public static readonly CharSprite NONE = new();
	public int FontIndex;
	public char Char;
	public FRect Offset;
	public float Advance;
	public object Texture;
}
