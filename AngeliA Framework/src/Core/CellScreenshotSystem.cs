using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class CellScreenshotSystem {




	#region --- SUB ---


	private class ScreenshotComparer : IComparer<Screenshot> {
		public static readonly ScreenshotComparer Instance = new();
		public int Compare (Screenshot a, Screenshot b) => b.CreatedDate.CompareTo(a.CreatedDate);
	}


	public class Screenshot {
		// Info
		public long CreatedDate;
		public string FilePath;
		// Content
		public IRect Range;
		public Color32 SkyTop;
		public Color32 SkyBottom;
		public CellRawData[] CellsWallpaper;
		public CellRawData[] CellsBehind;
		public CellRawData[] CellsShadow;
		public CellRawData[] CellsDefault;
	}


	#endregion




	#region --- VAR ---


	// Api
	public static int Count => Screenshots.Count;

	// Data
	private static readonly List<Screenshot> Screenshots = [];
	private static readonly List<CellRawData> CellCache = [];
	private static string MetaRoot = "";


	#endregion




	#region --- MSG ---



	[OnGameInitialize]
	internal static void OnGameInitialize () {
		MetaRoot = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "Screenshot");
	}


	[OnGameInitialize(1)]
	[OnSavingSlotChanged]
	internal static void ReloadFromFile () {
		Util.CreateFolder(MetaRoot);
		LoadAllScreenshotFromFile();
	}


	#endregion




	#region --- API ---


	public static Screenshot TakeScreenshot (IRect cameraRange) {
		var now = System.DateTime.UtcNow;
		var result = new Screenshot {
			CreatedDate = now.ToFileTimeUtc(),
			FilePath = Util.CombinePaths(MetaRoot, now.ToString("yyyy-dd-M--HH-mm-ss-fff")),
			SkyBottom = Sky.SkyTintBottomColor,
			SkyTop = Sky.SkyTintTopColor,
			Range = cameraRange,
			CellsWallpaper = GetRawCells(RenderLayer.WALLPAPER, cameraRange),
			CellsBehind = GetRawCells(RenderLayer.BEHIND, cameraRange),
			CellsShadow = GetRawCells(RenderLayer.SHADOW, cameraRange),
			CellsDefault = GetRawCells(RenderLayer.DEFAULT, cameraRange),
		};
		Screenshots.Add(result);
		Screenshots.Sort(ScreenshotComparer.Instance);
		SaveScreenshotToFile(result);
		return result;
		// Func
		static CellRawData[] GetRawCells (int layer, IRect cameraRange) {
			if (!Renderer.GetCells(layer, out var cells, out int count)) return [];
			CellCache.Clear();
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				if (!cell.GetGlobalBounds().Overlaps(cameraRange)) continue;
				CellCache.Add(cell.GetRawData());
			}
			return [.. CellCache];
		}
	}


	public static Screenshot GetScreenshot (int index) => Screenshots[index];


	public static void DeleteScreenshot (int index) {
		if (index < 0 || index >= Screenshots.Count) return;
		var shot = Screenshots[index];
		if (shot == null) return;
		Screenshots.RemoveAt(index);
		Util.DeleteFile(shot.FilePath);
	}


	public static void DrawScreenshot (Screenshot screenshot, IRect rect) => DrawScreenshot(screenshot, rect, Color32.WHITE);
	public static void DrawScreenshot (Screenshot screenshot, IRect rect, Color32 tint) {



	}


	#endregion




	#region --- LGC ---


	private static void LoadAllScreenshotFromFile () {
		Screenshots.Clear();




		Screenshots.Sort(ScreenshotComparer.Instance);
	}


	private static void SaveScreenshotToFile (Screenshot screenshot) {


	}


	#endregion




}
