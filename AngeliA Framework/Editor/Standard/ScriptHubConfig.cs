


namespace AngeliaFramework.Editor {



	public class AssetsScriptHub : IScriptHubConfig {
		public string[] Paths => new string[] { "Assets", };
		public string IgnoreFolders =>
			"Aseprite\n" +
			"Standard\n" +
			"AngeliA Framework\n" +
			"Third Party";
		public string Title => UnityEngine.Application.productName;
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => 0;
		public int Column => 1;
	}


	public class AsepriteHub : IScriptHubConfig {
		public string[] Paths => new string[] {
			"Assets",
			"Packages/com.moenengames.angeliaframework",
			"Packages/com.moenengames.angeliageneral",
			"Assets/AngeliA Framework",
			"Assets/AngeliA General",
		};
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
		public string IgnoreFolders => "Aseprite\nThird Party\nStandard\nGeneral";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -1024;
		public int Column => 3;
		public string GetFolderName (string name) {
			if (name.Equals("Editor")) {
				name = "zEditor";
			} else if (name.Equals("Framework")) {
				name = " Framework";
			}
			return name;
		}
	}



	public class AngeliaGeneralHab : IScriptHubConfig {
		public string Title => "AngeliA General";
		public string[] Paths => new string[] {
			"Packages/com.moenengames.angeliaframework/General",
			"Assets/AngeliA Framework/General",
		};
		public string IgnoreFolders => "Aseprite\nThird Party\nStandard";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -1024;
		public int Column => 2;
	}


}