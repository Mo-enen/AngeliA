
namespace AngeliA;


public class LanguageCode {
	public string Name { get; }
	public string DefaultValue { get; }
	public readonly int ID;
	public LanguageCode (string name, string defaultValue) {
		Name = name;
		DefaultValue = defaultValue;
		ID = name.AngeHash();
	}
	public static implicit operator LanguageCode ((string name, string defaultValue) value) => new(value.name, value.defaultValue);
	public static implicit operator string (LanguageCode code) => Language.Get(code.ID, code.DefaultValue);
}
