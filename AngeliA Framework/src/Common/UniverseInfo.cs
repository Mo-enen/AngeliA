namespace AngeliA;


/// <summary>
/// Representation of Info.json in universe folder
/// </summary>
[System.Serializable]
public class UniverseInfo : IJsonSerializationCallback {

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
