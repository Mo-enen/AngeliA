
namespace AngeliA;

public class LanguageCode (string name, string defaultValue) {
	public string Name { get; } = name;
	public string DefaultValue { get; } = defaultValue;
	public readonly int ID = name.AngeHash();
	public override string ToString () => Language.Get(ID, DefaultValue);
	public static implicit operator LanguageCode ((string name, string defaultValue) value) => new(value.name, value.defaultValue);
	public static implicit operator string (LanguageCode code) => Language.Get(code.ID, code.DefaultValue);
}
