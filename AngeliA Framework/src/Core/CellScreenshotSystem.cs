using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Core system to take screenshot based on rendering cells and save them into file
/// </summary>
public static class CellScreenshotSystem {




	#region --- SUB ---


	private class ScreenshotComparer : IComparer<Screenshot> {
		public static readonly ScreenshotComparer Instance = new();
		public int Compare (Screenshot a, Screenshot b) => b.CreatedDate.CompareTo(a.CreatedDate);
	}


	private class RawCellComparer : IComparer<RawCell> {
		public static readonly RawCellComparer Instance = new();
		public int Compare (RawCell a, RawCell b) {
			int result = a.Z.CompareTo(b.Z);
			return result != 0 ? result : a.Order.CompareTo(b.Order);
		}
	}


	/// <summary>
	/// Data to hold a single screenshot
	/// </summary>
	public class Screenshot {

		// Info
		public long CreatedDate;
		public string FilePath;
		/// <summary>
		/// True if the user want to prevent accidental deletion
		/// </summary>
		public bool Locked;

		// Content
		/// <summary>
		/// Global range of the rendering cells inside this screenshot
		/// </summary>
		public IRect Range;
		/// <summary>
		/// Sky gradient color on top
		/// </summary>
		public Color32 SkyTop;
		/// <summary>
		/// Sky gradient color on bottom
		/// </summary>
		public Color32 SkyBottom;
		internal RawCell[] CellsShadow;
		internal RawCell[] CellsDefault;

		// API
		internal void Read (BinaryReader reader) {

			Locked = reader.ReadBoolean();
			CreatedDate = reader.ReadInt64();

			Range.x = reader.ReadInt32();
			Range.y = reader.ReadInt32();
			Range.width = reader.ReadInt32();
			Range.height = reader.ReadInt32();

			SkyTop.r = reader.ReadByte();
			SkyTop.g = reader.ReadByte();
			SkyTop.b = reader.ReadByte();
			SkyTop.a = 255;
			SkyBottom.r = reader.ReadByte();
			SkyBottom.g = reader.ReadByte();
			SkyBottom.b = reader.ReadByte();
			SkyBottom.a = 255;

			CellsShadow = ReadCells(reader);
			CellsDefault = ReadCells(reader);

			// Func
			static RawCell[] ReadCells (BinaryReader reader) {
				int count = reader.ReadInt32();
				var result = new RawCell[count];
				for (int i = 0; i < count; i++) {
#pragma warning disable IDE0017
					var cell = new RawCell();
#pragma warning restore IDE0017
					cell.SpriteID = reader.ReadInt32();
					cell.TextChar = reader.ReadChar();
					cell.X = reader.ReadInt32();
					cell.Y = reader.ReadInt32();
					cell.Z = reader.ReadInt32();
					cell.Width = reader.ReadInt32();
					cell.Height = reader.ReadInt32();
					cell.Rotation1000 = reader.ReadInt32();
					cell.PivotX = reader.ReadSingle();
					cell.PivotY = reader.ReadSingle();
					cell.Color.r = reader.ReadByte();
					cell.Color.g = reader.ReadByte();
					cell.Color.b = reader.ReadByte();
					cell.Color.a = reader.ReadByte();
					cell.Shift.left = reader.ReadInt32();
					cell.Shift.right = reader.ReadInt32();
					cell.Shift.down = reader.ReadInt32();
					cell.Shift.up = reader.ReadInt32();
					cell.BorderSide = (Alignment)reader.ReadByte();
					result[i] = cell;
				}
				return result;
			}

		}

		internal void Write (BinaryWriter writer) {

			writer.Write((bool)Locked);
			writer.Write((long)CreatedDate);

			writer.Write((int)Range.x);
			writer.Write((int)Range.y);
			writer.Write((int)Range.width);
			writer.Write((int)Range.height);

			writer.Write((byte)SkyTop.r);
			writer.Write((byte)SkyTop.g);
			writer.Write((byte)SkyTop.b);
			writer.Write((byte)SkyBottom.r);
			writer.Write((byte)SkyBottom.g);
			writer.Write((byte)SkyBottom.b);

			WriteCells(CellsShadow, writer);
			WriteCells(CellsDefault, writer);

			// Func
			static void WriteCells (RawCell[] cells, BinaryWriter writer) {
				if (cells == null) {
					writer.Write((int)0);
				} else {
					writer.Write((int)cells.Length);
					for (int i = 0; i < cells.Length; i++) {
						var cell = cells[i];
						writer.Write((int)cell.SpriteID);
						writer.Write((char)cell.TextChar);
						writer.Write((int)cell.X);
						writer.Write((int)cell.Y);
						writer.Write((int)cell.Z);
						writer.Write((int)cell.Width);
						writer.Write((int)cell.Height);
						writer.Write((int)cell.Rotation1000);
						writer.Write((float)cell.PivotX);
						writer.Write((float)cell.PivotY);
						writer.Write((byte)cell.Color.r);
						writer.Write((byte)cell.Color.g);
						writer.Write((byte)cell.Color.b);
						writer.Write((byte)cell.Color.a);
						writer.Write((int)cell.Shift.left);
						writer.Write((int)cell.Shift.right);
						writer.Write((int)cell.Shift.down);
						writer.Write((int)cell.Shift.up);
						writer.Write((byte)cell.BorderSide);
					}
				}
			}
		}

	}


	#endregion




	#region --- VAR ---


	// Api
	/// <summary>
	/// Screenshot data count inside the current system
	/// </summary>
	public static int Count => Screenshots.Count;

	// Data
	private static readonly List<Screenshot> Screenshots = [];
	private static readonly List<RawCell> CellCache = [];
	private static string MetaRoot = "";
	private static IRect? RequiringScreenshotRange = null;
	private static int RequiringScreenshotFrame = int.MinValue;


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



	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (RequiringScreenshotRange.HasValue && Game.GlobalFrame >= RequiringScreenshotFrame) {
			TakeScreenshotImmediately(RequiringScreenshotRange.Value);
			RequiringScreenshotRange = null;
		}
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Take screenshot at given range in global space when all cells at this frame is rendered. Result will be saved into system list.
	/// </summary>
	/// <param name="cameraRange"></param>
	/// <param name="delay">Time delay in frame</param>
	public static void RequireTakeScreenshot (IRect cameraRange, int delay = 0) {
		RequiringScreenshotRange = cameraRange;
		RequiringScreenshotFrame = Game.GlobalFrame + delay;
	}


	/// <summary>
	/// Take screenshot at given range in global space no matter rendering cells are all exists for current frame
	/// </summary>
	/// <param name="cameraRange"></param>
	/// <param name="saveToSystem">True if save this screenshot into system list</param>
	/// <returns></returns>
	public static Screenshot TakeScreenshotImmediately (IRect cameraRange, bool saveToSystem = true) {
		var now = System.DateTime.UtcNow;
		var result = new Screenshot {
			Locked = false,
			CreatedDate = now.ToFileTimeUtc(),
			FilePath = Util.CombinePaths(MetaRoot, now.ToString("yyyy-dd-M--HH-mm-ss-fff")),
			SkyBottom = Sky.SkyTintBottomColor,
			SkyTop = Sky.SkyTintTopColor,
			Range = cameraRange,
			CellsShadow = GetRawCells(RenderLayer.SHADOW, cameraRange),
			CellsDefault = GetRawCells(RenderLayer.DEFAULT, cameraRange),
		};
		if (saveToSystem) {
			Screenshots.Add(result);
			Screenshots.Sort(ScreenshotComparer.Instance);
			SaveScreenshotToFile(result);
		}
		return result;
		// Func
		static RawCell[] GetRawCells (int layer, IRect cameraRange) {
			if (!Renderer.GetCells(layer, out var cells, out int count)) return [];
			CellCache.Clear();
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				if (!cell.GetGlobalBounds().Overlaps(cameraRange)) continue;
				var raw = cell.GetRawData();
				raw.Order = CellCache.Count;
				CellCache.Add(raw);
			}
			CellCache.Sort(RawCellComparer.Instance);
			return [.. CellCache];
		}
	}


	/// <summary>
	/// Get screenshot from system list at given index.
	/// </summary>
	public static Screenshot GetScreenshot (int index) => index >= 0 && index < Screenshots.Count ? Screenshots[index] : null;


	/// <summary>
	/// Delete screenshot from system list at given index
	/// </summary>
	/// <param name="index"></param>
	/// <param name="dontDeleteLocked">True if skip the locked ones</param>
	public static void DeleteScreenshot (int index, bool dontDeleteLocked = true) {
		if (index < 0 || index >= Screenshots.Count) return;
		var shot = Screenshots[index];
		if (shot == null) return;
		if (shot.Locked && dontDeleteLocked) return;
		Screenshots.RemoveAt(index);
		Util.DeleteFile(shot.FilePath);
	}


	/// <summary>
	/// Lock/unlock screenshot in system list at given index
	/// </summary>
	public static void SetScreenshotLock (int index, bool locked) {
		if (index < 0 || index >= Screenshots.Count) return;
		var shot = Screenshots[index];
		if (shot == null || shot.Locked == locked) return;
		shot.Locked = locked;
		SaveScreenshotToFile(shot);
	}


	/// <inheritdoc cref="DrawScreenshot(Screenshot, IRect, FRect, Color32, int, int, bool)"/>
	public static void DrawScreenshot (Screenshot screenshot, IRect rect, int z = 0, int layer = RenderLayer.UI, bool fit = true) => DrawScreenshot(screenshot, rect, new FRect(0, 0, 1, 1), Color32.WHITE, z, layer, fit);
	/// <inheritdoc cref="DrawScreenshot(Screenshot, IRect, FRect, Color32, int, int, bool)"/>
	public static void DrawScreenshot (Screenshot screenshot, IRect rect, FRect sourceRange, int z = 0, int layer = RenderLayer.UI, bool fit = true) => DrawScreenshot(screenshot, rect, sourceRange, Color32.WHITE, z, layer, fit);
	/// <inheritdoc cref="DrawScreenshot(Screenshot, IRect, FRect, Color32, int, int, bool)"/>
	public static void DrawScreenshot (Screenshot screenshot, IRect rect, Color32 tint, int z = 0, int layer = RenderLayer.UI, bool fit = true) => DrawScreenshot(screenshot, rect, new FRect(0, 0, 1, 1), tint, z, layer, fit);
	/// <summary>
	/// Render given screen shot on screen
	/// </summary>
	/// <param name="screenshot">Screenshot data</param>
	/// <param name="rect">Global range to display this screenshot</param>
	/// <param name="sourceRange">Which part of the screenshot should be display. ((0, 0, 1, 1) means all of them. (0, 0, 0.5, 1) means left half of them)</param>
	/// <param name="tint">Color tint</param>
	/// <param name="z">Z value for sort rendering cells</param>
	/// <param name="layer">Rendering layer to draw into. Use RenderLayer.XXX to get this value.</param>
	/// <param name="fit">True if resize the result without changing the aspect ratio</param>
	public static void DrawScreenshot (Screenshot screenshot, IRect rect, FRect sourceRange, Color32 tint, int z = 0, int layer = RenderLayer.UI, bool fit = true) {
		if (screenshot == null) return;
		if (sourceRange.width.AlmostZero() || sourceRange.height.AlmostZero()) return;
		int oldLayer = Renderer.CurrentLayerIndex;
		try {
			var screenshotRange = screenshot.Range;
			Renderer.SetLayer(layer);
			var photoRect = fit ?
				rect.Fit(screenshotRange.width, screenshotRange.height) :
				rect.Envelope(screenshotRange.width, screenshotRange.height);
			// Draw Sky BG
			Renderer.DrawPixel(
				fit ? photoRect : rect,
				Color32.Lerp(screenshot.SkyTop, screenshot.SkyBottom, 0.5f),
				z: z - 1
			);
			// Draw Cells
			int cellStart = Renderer.GetUsedCellCount();
			using var _ = new ClampCellsScope(fit ? photoRect : rect);
			var targetRange = IRect.MinMaxRect(
				Util.LerpUnclamped(screenshotRange.x, screenshotRange.xMax, sourceRange.x).RoundToInt(),
				Util.LerpUnclamped(screenshotRange.y, screenshotRange.yMax, sourceRange.y).RoundToInt(),
				Util.LerpUnclamped(screenshotRange.x, screenshotRange.xMax, sourceRange.xMax).RoundToInt(),
				Util.LerpUnclamped(screenshotRange.y, screenshotRange.yMax, sourceRange.yMax).RoundToInt()
			);
			if (!fit) {
				targetRange = targetRange.Fit(rect.width, rect.height);
			}
			DrawCells(screenshot.CellsShadow, targetRange, tint, z);
			DrawCells(screenshot.CellsDefault, targetRange, tint, z);
			// Remap Cell Position
			if (Renderer.GetCells(out var cells, out int count)) {
				int cellEnd = Renderer.GetUsedCellCount().LessOrEquel(count);
				FrameworkUtil.RemapCells(
					cells, cellStart, cellEnd,
					screenshotRange,
					rect.ScaleFrom(
						1f / sourceRange.width,
						1f / sourceRange.height,
						(rect.x + rect.width * sourceRange.center.x).RoundToInt(),
						(rect.y + rect.height * sourceRange.center.y).RoundToInt()
					),
					fit: fit
				);
			}
			// Func
			static void DrawCells (RawCell[] cells, IRect targetRange, Color32 tint, int z) {
				if (!Renderer.TryGetSprite(Const.PIXEL, out var sprite, true)) return;
				for (int i = 0; i < cells.Length; i++) {
					var raw = cells[i];
					var bounds = raw.GetGlobalBounds();
					if (!bounds.Overlaps(targetRange)) continue;
					var cell = Renderer.Draw(sprite, default);
					cell.LoadFromRawData(raw);
					cell.Color *= tint;
					cell.Z = z;
				}
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
		Renderer.SetLayer(oldLayer);
	}


	#endregion




	#region --- LGC ---


	private static void LoadAllScreenshotFromFile () {
		Screenshots.Clear();
		foreach (string path in Util.EnumerateFiles(MetaRoot, true, "*")) {
			using var stream = File.Open(path, FileMode.Open);
			using var reader = new BinaryReader(stream);
			var shot = new Screenshot();
			while (reader.NotEnd()) {
				shot.Read(reader);
				shot.FilePath = path;
			}
			Screenshots.Add(shot);
		}
		Screenshots.Sort(ScreenshotComparer.Instance);
	}


	private static void SaveScreenshotToFile (Screenshot screenshot) {
		string path = screenshot.FilePath;
		using var stream = File.Open(path, FileMode.Create);
		using var writer = new BinaryWriter(stream);
		screenshot.Write(writer);
	}


	#endregion




}
