using AngeliaFramework;
using AngeliaFramework.Editor;


namespace AngeliaGeneral.Editor {

	public class GeneralPackageHab : IScriptHubConfig {
		public string Title => "AngeliA General";
		public string[] Paths => new string[] {
			"Packages/com.moenengames.angeliageneral",
			"Assets/AngeliA General",
		};
		public string IgnoreFolders => "Editor";
		public string IgnoreFiles => "";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -1024;
		public int Column => 2;
		public string GetFileName (string name) {
			if (!string.IsNullOrEmpty(name) && name[0] == 'e') {
				return name[1..];
			}
			return name;
		}
	}


}