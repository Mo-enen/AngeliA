namespace AngeliA;


/// <summary></summary>
public enum ProjectType {
	/// <summary>
	/// Project with coded logic, pixel artwork and audio etc...
	/// </summary>
	Game,
	/// <summary>
	/// Project with only pixel artwork
	/// </summary>
	Artwork,
	/// <summary>
	/// Project with only pixel artwork for theme of AngeliA Engine
	/// </summary>
	EngineTheme,
}


/// <summary>
/// Representation of Info.json in universe folder
/// </summary>
[System.Serializable]
public class UniverseInfo : IJsonSerializationCallback {

	/// <summary>
	/// Official name of this project in English
	/// </summary>
	public string ProductName = "";
	/// <summary>
	/// Developer name of this project in English
	/// </summary>
	public string DeveloperName = "";
	/// <summary>
	/// Type of this project
	/// </summary>
	public ProjectType ProjectType = ProjectType.Game;
	/// <summary>
	/// Major version of this project
	/// </summary>
	public int MajorVersion = 0;
	/// <summary>
	/// Minor version of this project
	/// </summary>
	public int MinorVersion = 0;
	/// <summary>
	/// Patch version of this project
	/// </summary>
	public int PatchVersion = 0;
	/// <summary>
	/// Which version of AngeliA Engine published this project
	/// </summary>
	public uint EngineBuildVersion = 0;
	/// <summary>
	/// Require map editor from AngeliA Engine when under development
	/// </summary>
	public bool UseMapEditor = true;
	/// <summary>
	/// Use the global lighting system
	/// </summary>
	public bool UseLightingSystem = false;
	/// <summary>
	/// Save map file changes made by the player
	/// </summary>
	public bool ReadonlyMap = false;
	/// <summary>
	/// Use cheat code after release
	/// </summary>
	public bool AllowCheatCode = false;
	/// <summary>
	/// Allow player press start button (esc) to pause
	/// </summary>
	public bool AllowPause = true;
	/// <summary>
	/// Show quit option inside pause menu
	/// </summary>
	public bool AllowQuitFromMenu = true;
	/// <summary>
	/// Show restart option inside pause menu
	/// </summary>
	public bool AllowRestartFromMenu = true;
	/// <summary>
	/// Scale ui elements based on the monitor height instead of application window height
	/// </summary>
	public bool ScaleUiBasedOnMonitor = true;
	/// <summary>
	/// Size ratio of the view rect. (1000 means 1:1, 2000 means 2:1)
	/// </summary>
	public int ViewRatio = 2000;
	/// <summary>
	/// Default view rect height in global size
	/// </summary>
	public int DefaultViewHeight = Const.CEL * 26;
	/// <summary>
	/// Minimal view rect height in global size
	/// </summary>
	public int MinViewHeight = Const.CEL * 16;
	/// <summary>
	/// Maximal view rect height in global size
	/// </summary>
	public int MaxViewHeight = Const.CEL * 60;
	/// <summary>
	/// Use pixel style lighting
	/// </summary>
	public bool LightMap_PixelStyle = false;
	/// <summary>
	/// Adjust the shadow generating influence between "Solid block itself" and "Global sun light"
	/// </summary>
	public float LightMap_SelfLerp = 0.88f;
	/// <summary>
	/// How much does solid blocks luminous
	/// </summary>
	public float LightMap_SolidIlluminance = 1f;
	/// <summary>
	/// How much does air luminous during daytime
	/// </summary>
	public float LightMap_AirIlluminanceDay = 0.95f;
	/// <summary>
	/// How much does air luminous during nighttime
	/// </summary>
	public float LightMap_AirIlluminanceNight = 0.3f;
	/// <summary>
	/// How much does background blocks luminous
	/// </summary>
	public float LightMap_BackgroundTint = 0.5f;
	/// <summary>
	/// How much does light remain after it hit solid blocks
	/// </summary>
	public float LightMap_LevelIlluminateRemain = 0.3f;
	/// <summary>
	/// Parallax amount of the behind map layer. (1000 means no parallax, 2000 means behind move 2 times faster)
	/// </summary>
	public int WorldBehindParallax = 1300;
	/// <summary>
	/// Transparent amount of the behind map layer. (255 means not tramsparent, 0 means full tramsparent)
	/// </summary>
	public byte WorldBehindAlpha = 64;
	public bool RequireFixScriptNamesWhenAnalyse = false;
	/// <summary>
	/// Last opened atlas index by AngeliA Engine
	/// </summary>
	public int LastOpenAtlasIndex = 0;
	/// <summary>
	/// Last edit view rect position from map editor
	/// </summary>
	public Int3 LastEdittingViewPos = default;
	/// <summary>
	/// Last edit view rect height from map editor
	/// </summary>
	public int LastEdittingViewHeight = -1;
	/// <summary>
	/// True if the game require rendering cell pixel perfect
	/// </summary>
	public bool UsePixelPerfectRendering = false;

	public void OnAfterLoadedFromDisk () => Valid(true);
	public void OnBeforeSaveToDisk () => Valid(true);
	public void Valid (bool minViewSizeFirst) {
		ViewRatio = ViewRatio.Clamp(250, 4000);
		if (minViewSizeFirst) {
			MinViewHeight = MinViewHeight.Clamp(Const.CEL * 16, Const.CEL * 1024);
			MaxViewHeight = MaxViewHeight.Clamp(MinViewHeight, Const.CEL * 1024);
		} else {
			MaxViewHeight = MaxViewHeight.Clamp(Const.CEL * 16, Const.CEL * 1024);
			MinViewHeight = MinViewHeight.Clamp(Const.CEL * 16, MaxViewHeight);
		}
		DefaultViewHeight = DefaultViewHeight.Clamp(MinViewHeight, MaxViewHeight);
		WorldBehindParallax = WorldBehindParallax.Clamp(300, 3000);
	}

}
