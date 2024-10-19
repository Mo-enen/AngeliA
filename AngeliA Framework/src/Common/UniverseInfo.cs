namespace AngeliA;


public enum ProjectType {
	Game,
	Artwork,
	EngineTheme,
}


[System.Serializable]
public class UniverseInfo : IJsonSerializationCallback {
	public string ProductName = "";
	public string DeveloperName = "";
	public ProjectType ProjectType = ProjectType.Game;
	public int MajorVersion = 0;
	public int MinorVersion = 0;
	public int PatchVersion = 0;
	public uint EngineBuildVersion = 0;
	public bool UseProceduralMap = false;
	public bool UseMapEditor = true;
	public bool UseLightingSystem = true;
	public bool AllowCheatCode = false;
	public bool AllowPause = true;
	public bool AllowQuitFromMenu = true;
	public bool AllowRestartFromMenu = true;
	public bool ScaleUiBasedOnMonitor = true;
	public int ViewRatio = 2000;
	public int DefaultViewHeight = Const.CEL * 26;
	public int MinViewHeight = Const.CEL * 16;
	public int MaxViewHeight = Const.CEL * 60;
	public bool LightMap_PixelStyle = false;
	public float LightMap_SelfLerp = 0.88f;
	public float LightMap_SolidIlluminance = 1f;
	public float LightMap_AirIlluminanceDay = 0.95f;
	public float LightMap_AirIlluminanceNight = 0.3f;
	public float LightMap_BackgroundTint = 0.5f;
	public float LightMap_LevelIlluminateRemain = 0.3f;
	public int WorldBehindParallax = 1300;
	public byte WorldBehindAlpha = 64;
	public bool RequireFixScriptNamesWhenAnalyse = false;

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
