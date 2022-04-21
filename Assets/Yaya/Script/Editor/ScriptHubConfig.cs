using Moenen.Standard;


namespace Yaya.Editor {
	public class YayaScriptHub : IScriptHubConfig {
		public string[] Paths => new string[] { "Assets", };
		public string IgnoreFolders => "";
		public string IgnoreFiles => "";
		public string Title => "Yaya";
		public IScriptHubConfig.FileExtension[] FileExtensions => new IScriptHubConfig.FileExtension[]{
				new ("cs", "", true),
			};
		public int Order => 0;
		public int Column => 3;
	}


	public class YayaArtworkHub : IScriptHubConfig {
		public string[] Paths => new string[] { "Assets", };
		public string IgnoreFolders => "";
		public string IgnoreFiles => "";
		public string Title => "Yaya Artwork";
		public IScriptHubConfig.FileExtension[] FileExtensions => new IScriptHubConfig.FileExtension[]{
				new ("ase", "Ase", true),
				new ("aseprite", "Aseprite", true),
			};
		public int Order => 0 + 1;

	}
}