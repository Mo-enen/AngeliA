using AngeliaFramework;


namespace AngeliaForUnity.Editor {


	public class FrameworkHab : IScriptHubConfig {
		public string Title => "Framework";
		public string[] Paths => new string[] {
			"Packages/com.moenengames.angeliaframework/Framework",
		};
		public string IgnoreFolders => "Aseprite\nThird Party\nStandard";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -4096;
		public int Column => 1;
		public string GetFolderName (string name) {
			if (name.Equals("Editor")) {
				name = "zEditor";
			} else if (name.Equals("Framework")) {
				name = " Framework";
			}
			return name;
		}
	}


	public class ImplementHab : IScriptHubConfig {
		public string Title => "Implement";
		public string[] Paths => new string[] {
			"Packages/com.moenengames.angeliaframework/Implement",
		};
		public string IgnoreFolders => "Aseprite\nThird Party\nStandard";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -4095;
		public int Column => 4;
	}


	public class ForUnityHab : IScriptHubConfig {
		public string Title => "For Unity";
		public string[] Paths => new string[] {
			"Packages/com.moenengames.angeliaforunity",
		};
		public string IgnoreFolders => "Aseprite\nThird Party\nStandard";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
		};
		public int Order => -2048;
		public int Column => 1;
		public string GetFolderName (string name) {
			if (name.Equals("Editor")) {
				name = "zEditor";
			} else if (name.Equals("Framework")) {
				name = " Framework";
			}
			return name;
		}
	}




	public class AssetsScriptHub : IScriptHubConfig {
		public string Title => UnityEngine.Application.productName;
		public string[] Paths => new string[] { "Assets", };
		public string IgnoreFolders => "Aseprite\nStandard\nAngeliA Framework\nThird Party";
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.cs", "", true),
			new ($"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}", "Conversation", true),
		};
		public int Order => 0;
		public int Column => 1;
	}


	public class ArtworkHub : IScriptHubConfig {
		public string Title => "Artwork";
		public string[] Paths => new string[] { 
			"Universe/Artwork", 
		};
		public IScriptHubConfig.SearchPattern[] SearchPatterns => new IScriptHubConfig.SearchPattern[]{
			new ("*.ase", "Ase", true),
			new ("*.aseprite", "Aseprite", true),
		};
		public int Order => 4096;
	}



}