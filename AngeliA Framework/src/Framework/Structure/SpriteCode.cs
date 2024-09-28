namespace AngeliA;
public class SpriteCode (string name) {
	public string Name { get; } = name;
	public readonly int ID = name.AngeHash();
	public static implicit operator SpriteCode (string value) => new(value);
	public static implicit operator int (SpriteCode code) => code.ID;
}