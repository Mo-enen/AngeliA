using Moenen.Standard;


namespace Yaya.Editor {
	public class YayaScriptHub : IScriptHubConfig {
		public string[] Paths => new string[] { "Assets/Yaya", };
		public string IgnoreFolders => "";
		public string IgnoreFiles => "";
		public string Title => "Yaya";
		public IScriptHubConfig.FileExtension[] FileExtensions => new IScriptHubConfig.FileExtension[]{
				new ("cs", "", true),
			};
		public int Order => 0;
		public int Column => 3;
		public string GetFileName (string name) => name.StartsWith('e') || name.StartsWith('s') || name.StartsWith('a') ? name[1..] : name;
		public string GetFolderName (string name) {
			if (name.Equals("Entity")) {
				name = "z.Entity";
			}
			if (name.Equals("Editor")) {
				name = "zz.Editor";
			}
			return name;
		}
	}


	public class YayaArtworkHub : IScriptHubConfig {
		public string[] Paths => new string[] { "Assets/Yaya", };
		public string IgnoreFolders => "";
		public string IgnoreFiles => "";
		public string Title => "Yaya Artwork";
		public IScriptHubConfig.FileExtension[] FileExtensions => new IScriptHubConfig.FileExtension[]{
				new ("ase", "Ase", true),
				new ("aseprite", "Aseprite", true),
			};
		public int Order => 1;
		public string GetFileName (string name) => name.StartsWith('e') || name.StartsWith('a') ? name[1..] : name;
	}
}