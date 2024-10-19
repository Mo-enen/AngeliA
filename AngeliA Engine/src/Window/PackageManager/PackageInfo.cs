using System.Text.Json.Serialization;

namespace AngeliaEngine;

public class PackageInfo {

	public bool AnyResourceFounded => DllFounded || SheetFounded || ThemeFounded || LanguageFounded;

	public string PackageName;
	public string DisplayName;
	public string CreatorName;
	public string Description;
	public int Priority;

	[JsonIgnore] public bool Installed;
	[JsonIgnore] public object IconTexture;
	[JsonIgnore] public bool IsBuiltIn;

	[JsonIgnore] public string DllPath;
	[JsonIgnore] public string SheetPath;
	[JsonIgnore] public string ThemeFolder;
	[JsonIgnore] public string LanguageFolder;

	[JsonIgnore] public bool DllFounded;
	[JsonIgnore] public bool SheetFounded;
	[JsonIgnore] public bool ThemeFounded;
	[JsonIgnore] public bool LanguageFounded;

}
