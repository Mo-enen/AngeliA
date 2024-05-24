namespace AngeliA;

public class CharSprite {
	public int FontIndex;
	public char Char;
	public FRect Offset;
	public float Advance;
	public object Texture;
	~CharSprite () => Game.UnloadTexture(Texture);
}
