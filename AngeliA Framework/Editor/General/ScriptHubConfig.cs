


namespace AngeliaFramework.Editor {



	public class AssetsScriptHub : IScriptHubConfig {
		public string[] Paths => new string[] { "Assets", };
		public string IgnoreFolders =>
			"Aseprite\n" +
			"Standard\n" +
			"AngeliA Framework\n" +
			"Third Party";
		public string IgnoreFiles => "";
		public string Title => UnityEngine.Application.productName;
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => 0;
		public int Column => 2;
		public string GetFileName (string name) {
			if (!string.IsNullOrEmpty(name) && name[0] == 'e') {
				return name[1..];
			}
			return name;
		}
	}


	public class AsepriteHub : IScriptHubConfig {
		public string[] Paths => new string[] {
			"Assets",
			"Packages/com.moenengames.angeliaframework",
			"Packages/com.moenengames.angeliageneral",
			"Assets/AngeliA Framework",
			"Assets/AngeliA General",
		};
		public string IgnoreFolders => "";
		public string IgnoreFiles => "";
		public string Title => "Artwork";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.ase", "Ase", true),
			new ("*.aseprite", "Aseprite", true),
			new ("*#aa*.png", "PNG", true),
		};
		public int Order => 1;
	}


	public class AngeliaPackageHab : IScriptHubConfig {
		public string Title => "AngeliA Framework";
		public string[] Paths => new string[] {
			"Packages/com.moenengames.angeliaframework",
			"Assets/AngeliA Framework",
		};
		public string IgnoreFolders => "Aseprite\nThird Party\nStandard";
		public string IgnoreFiles => "";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -1024;
		public int Column => 2;
		public string GetFolderName (string name) {
			if (name.Equals("Editor")) {
				name = "zEditor";
			} else if (name.Equals("Framework")) {
				name = " Framework";
			}
			return name;
		}
	}


}