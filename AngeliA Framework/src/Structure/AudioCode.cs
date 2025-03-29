namespace AngeliA;
/// <summary>
/// Quick setup for an audio ID
/// </summary>
/// <example><code>
/// private static readonly AudioCode AudioCodeName = "Name of audio file without extension";
/// </code></example>
public class AudioCode (string name) {
	public string Name { get; } = name;
	public readonly int ID = name.AngeHash();
	public static implicit operator AudioCode (string value) => new(value);
	public static implicit operator int (AudioCode code) => code.ID;
}