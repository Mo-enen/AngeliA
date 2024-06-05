
namespace AngeliA;


public class SpriteCode {
	public string Name { get; }
	public readonly int ID;
	public SpriteCode (string name) {
		Name = name;
		ID = name.AngeHash();
	}
	public static implicit operator SpriteCode (string value) => new(value);
	public static implicit operator int (SpriteCode code) => code.ID;
}
