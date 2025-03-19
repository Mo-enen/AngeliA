namespace AngeliA;
public class AudioCode (string name) {
	public string Name { get; } = name;
	public readonly int ID = name.AngeHash();
	public static implicit operator AudioCode (string value) => new(value);
	public static implicit operator int (AudioCode code) => code.ID;
}