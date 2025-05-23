﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Core system for draw artwork on screen for current frame
/// </summary>
public static class Renderer {




	#region --- SUB ---


	internal class CellComparer : IComparer<Cell> {
		public static readonly CellComparer Instance = new();
		public int Compare (Cell a, Cell b) =>
			a.Z < b.Z ? -1 :
			a.Z > b.Z ? 1 :
			a.Order < b.Order ? -1 :
			a.Order > b.Order ? 1 :
			0;
	}


	private class Layer {
		public int Count => Util.Min(Cells.Length, FocusedCell >= 0 ? FocusedCell : Cells.Length);
		public Cell[] Cells;
		public int CellCount;
		public int FocusedCell;
		public int PrevCellCount;
		public int SortedIndex;
		public int SortingOrder;
		public Color32 LayerTint;
		public void ZSort () {
			if (SortedIndex < Count - 1) {
				var span = new Span<Cell>(Cells);
				Util.QuickSort(span, SortedIndex, Count - 1, CellComparer.Instance);
			}
			SortedIndex = Count;
		}
		public void ReverseUnsorted () {
			if (SortedIndex < Count) {
				System.Array.Reverse(Cells, SortedIndex, Count - SortedIndex);
				SortedIndex = Count;
			}
		}
		public void AbandonZSort () => SortedIndex = Count;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly bool[] DEFAULT_PART_IGNORE = [false, false, false, false, false, false, false, false, false,];
	private static readonly Cell[] SLICE_RESULT = new Cell[9];

	// Event
	[OnMainSheetReload] internal static Action OnMainSheetLoaded;

	// Api
	/// <summary>
	/// Current rect position in global space for rendering the screen
	/// </summary>
	public static IRect CameraRect { get; private set; } = new IRect(0, 0, 1, 1);
	/// <summary>
	/// Local range for the actual range for content on screen (black bar appears when window too wide)
	/// </summary>
	public static FRect CameraRange { get; private set; } = new(0, 0, 1f, 1f);
	/// <summary>
	/// Rendering rect position in screen position
	/// </summary>
	public static IRect ScreenRenderRect { get; private set; }
	/// <summary>
	/// Total rendering layer count
	/// </summary>
	public static int LayerCount => Layers.Length;
	/// <summary>
	/// Index of current using artwork sheet
	/// </summary>
	public static int CurrentSheetIndex {
		get => _CurrentSheetIndex;
		set {
			if (value != _CurrentSheetIndex) {
				_CurrentSheetIndex = value;
				CurrentSheet = CurrentSheetIndex < 0 || CurrentSheetIndex >= AltSheets.Count ? MainSheet : AltSheets[CurrentSheetIndex];
			}
		}
	}
	/// <summary>
	/// Index of current using rendering layer
	/// </summary>
	public static int CurrentLayerIndex { get; private set; } = 0;
	/// <summary>
	/// Index of current using font
	/// </summary>
	public static int CurrentFontIndex { get; private set; } = 0;
	/// <summary>
	/// Total alt sheet count
	/// </summary>
	public static int AltSheetCount => AltSheets.Count;
	/// <summary>
	/// True if the system is ready to use
	/// </summary>
	public static bool IsReady { get; private set; } = false;
	/// <summary>
	/// Instance of current using artwork sheet
	/// </summary>
	public static Sheet CurrentSheet { get; private set; }
	/// <summary>
	/// Instance of main/default artwork sheet
	/// </summary>
	public static readonly Sheet MainSheet = new(ignoreTextureAndPixels: Game.IgnoreArtworkPixels);
	internal static float CameraRestrictionRate { get; private set; } = 1f;

	// Data
	private static readonly List<Sheet> AltSheets = [];
	private static readonly Layer[] Layers = new Layer[RenderLayer.COUNT];
	private static readonly Dictionary<Int2, CharSprite> CharSpritePool = [];
	private static readonly Dictionary<int, int> FontIdIndexMap = [];
	private static int _CurrentSheetIndex = -1;
	private static bool IsDrawing = false;
	private static long MainSheetFileModifyDate = 0;
	private static string MainSheetFilePath = "";


	#endregion




	#region --- MSG ---


	// Init
	[OnGameInitialize(-4096)]
	internal static void Initialize () {

		// Load Capacity from Attribute
		var capacities = new int[RenderLayer.COUNT];
		RenderLayer.DEFAULT_CAPACITY.CopyTo(capacities, 0);
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<RendererLayerCapacityAttribute>()) {
			if (att.Layer < 0 || att.Layer >= RenderLayer.COUNT) continue;
			capacities[att.Layer] = att.Capacity;
		}

		// Create Layers
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			int capacity = capacities[i];
			int order = i;
			Layers[i] = new Layer {
				Cells = new Cell[capacity].FillWithNewValue(),
				CellCount = capacity,
				FocusedCell = 0,
				PrevCellCount = 0,
				SortedIndex = 0,
				SortingOrder = order,
				LayerTint = Color32.WHITE,
			};
		}

		// Load Sheet
		CurrentSheet = MainSheet;
		LoadMainSheet();

		// End
		IsReady = true;

	}


#if DEBUG
	[OnGameFocused]
	internal static void OnGameFocused () {
		// Reload Main Sheet on Changed
		long date = Util.GetFileModifyDate(MainSheetFilePath);
		if (date > MainSheetFileModifyDate) {
			LoadMainSheet();
			Util.DeleteFolder(Universe.BuiltIn.SlotCharacterRenderingConfigRoot);
			PoseCharacterRenderer.ReloadRenderingConfigPoolFromFileAndSheet();
			//Debug.Log("Artwork sheet reloaded from file.");
		}
	}
#endif


	// Update
	[OnGameUpdate(-2048)]
	internal static void UpdateCameraRect () {

		var viewRect = Stage.ViewRect;

		// Ratio
		float ratio = (float)Game.ScreenWidth / Game.ScreenHeight;
		float maxRatio = Game.IsToolApplication ?
			float.MaxValue - 1f :
			Universe.BuiltInInfo.ViewRatio / 1000f;
		var rect = new FRect(0f, 0f, 1f, 1f);
		if (ratio > maxRatio) {
			rect = new FRect(0.5f - 0.5f * maxRatio / ratio, 0f, maxRatio / ratio, 1f);
			CameraRestrictionRate = maxRatio / ratio;
		} else {
			CameraRestrictionRate = 1f;
		}
		CameraRange = rect;

		// Camera Rect
		float cameraAspect = Util.Min(ratio, maxRatio);
		var cRect = new IRect(
			viewRect.x,
			viewRect.y,
			(int)(viewRect.height * cameraAspect),
			viewRect.height
		);
		int cOffsetX = (viewRect.width - cRect.width) / 2;
		cRect.x += cOffsetX;
		CameraRect = cRect;

	}


	[OnGameUpdatePauseless(-2048)]
	internal static void PausingUpdate () {
		if (Game.IsPlaying) return;
		UpdateCameraRect();
	}


	[OnGameUpdatePauseless(-4096)]
	internal static void OnGameUpdatePauselessLate () {
		int width = Game.ScreenWidth;
		int height = Game.ScreenHeight;
		ScreenRenderRect = CameraRange.x.AlmostZero() ?
			new IRect(0, 0, width, height) :
			new IRect(
				Util.LerpUnclamped(0, width, CameraRange.x).RoundToInt(),
				0,
				(width * CameraRange.width).RoundToInt(),
				height
			);
	}


	[OnGameUpdatePauseless(32)]
	internal static void RenderAllLayers () {
		IsDrawing = false;
		Game.BeforeAllLayersUpdate();
		for (int i = 0; i < Layers.Length; i++) {
			try {
				var layer = Layers[i];
				if (i != RenderLayer.UI) {
					layer.ZSort();
				}
				int prevCount = layer.Count;
				if (Game.PauselessFrame < 4) continue;
				Game.OnLayerUpdate(i, layer.Cells, layer.Count);
				layer.PrevCellCount = prevCount;
			} catch (Exception ex) { Debug.LogException(ex); }
		}
		Game.AfterAllLayersUpdate();
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () => MainSheet.Clear();


	[OnGameUpdate(-512)]
	internal static void BeginDraw () {
		IsDrawing = true;
		Cell.EMPTY.Sprite = null;
		Cell.EMPTY.TextSprite = null;
		Cell.EMPTY.Color = Color32.CLEAR;
		SetLayerToDefault();
		CurrentSheetIndex = -1;
		CurrentFontIndex = 0;
		CurrentLayerIndex = RenderLayer.DEFAULT;
		for (int i = 0; i < Layers.Length; i++) {
			var layer = Layers[i];
			layer.LayerTint = Color32.WHITE;
			if (Game.IsPlaying || i == RenderLayer.UI) {
				layer.FocusedCell = 0;
				layer.SortedIndex = 0;
			}
		}
	}


	[OnGameUpdatePauseless(-512)]
	internal static void UpdatePausing () {
		if (Game.IsPlaying) return;
		BeginDraw();
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Reset internal pool for rendering character and unload the textures for them
	/// </summary>
	public static void ClearCharSpritePool () {
		foreach (var (_, sprite) in CharSpritePool) {
			Game.UnloadTexture(sprite.Texture);
		}
		CharSpritePool.Clear();
	}


	// Sheet
	/// <summary>
	/// Get texture object for given sprite from sheet
	/// </summary>
	/// <typeparam name="T">Type of texture object</typeparam>
	/// <param name="spriteID"></param>
	/// <param name="sheetIndex"></param>
	/// <param name="texture"></param>
	/// <returns>True if the texture founded</returns>
	public static bool TryGetTextureFromSheet<T> (int spriteID, int sheetIndex, out T texture) {
		var sheet = sheetIndex < 0 || sheetIndex >= AltSheets.Count ? MainSheet : AltSheets[sheetIndex];
		if (sheet.TexturePool.TryGetValue(spriteID, out object textureObj) && textureObj is T result) {
			texture = result;
			return true;
		} else {
			texture = default;
			return false;
		}
	}


	/// <summary>
	/// Load main sheet from built-in path
	/// </summary>
	public static void LoadMainSheet () {

		string gameSheetPath = Universe.BuiltIn.GameSheetPath;
		string gameSheetName = Util.GetNameWithExtension(gameSheetPath);

		// Load Sheet
		MainSheetFileModifyDate = 0;
		MainSheetFilePath = "";
		if (!MainSheet.LoadFromDisk(gameSheetPath)) return;

		MainSheetFilePath = gameSheetPath;
		MainSheetFileModifyDate = Util.GetFileModifyDate(gameSheetPath);

		// Load all Sub Sheets
		MainSheet.CombineAllSheetInFolder(Universe.BuiltIn.SheetRoot, false, gameSheetName);

		// Event
		OnMainSheetLoaded?.InvokeAsEvent();
	}


	/// <summary>
	/// Add alt sheet into the system
	/// </summary>
	/// <param name="sheet"></param>
	/// <returns>Index of the added alt sheet</returns>
	public static int AddAltSheet (Sheet sheet) {
		AltSheets.Add(sheet);
		return AltSheets.Count - 1;
	}


	/// <summary>
	/// Remove alt sheet at index from system
	/// </summary>
	public static void RemoveAltSheet (int index) => AltSheets.RemoveAt(index);


	/// <summary>
	/// Get instance of the alt sheet at given index
	/// </summary>
	public static Sheet GetAltSheet (int index) => AltSheets[index];


	// Layer
	/// <summary>
	/// Set current using layer. Use RenderLayer.XXX to get this value.
	/// </summary>
	public static void SetLayer (int index) {
		if (index < 0) {
			CurrentLayerIndex = 1;
		} else {
			CurrentLayerIndex = index.Clamp(0, Layers.Length - 1);
		}
	}
	public static void SetLayerToWallpaper () => CurrentLayerIndex = RenderLayer.WALLPAPER;
	public static void SetLayerToBehind () => CurrentLayerIndex = RenderLayer.BEHIND;
	public static void SetLayerToShadow () => CurrentLayerIndex = RenderLayer.SHADOW;
	public static void SetLayerToDefault () => CurrentLayerIndex = RenderLayer.DEFAULT;
	public static void SetLayerToColor () => CurrentLayerIndex = RenderLayer.COLOR;
	public static void SetLayerToMultiply () => CurrentLayerIndex = RenderLayer.MULT;
	public static void SetLayerToAdditive () => CurrentLayerIndex = RenderLayer.ADD;

	/// <summary>
	/// Clear the whole render layer
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	public static void ResetLayer (int layerIndex) {
		var layer = Layers[layerIndex];
		layer.FocusedCell = 0;
		layer.SortedIndex = 0;
	}

	/// <summary>
	/// Sort cells inside layer with default comparer
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	public static void SortLayer (int layerIndex) => Layers[layerIndex].ZSort();

	/// <summary>
	/// Sort cells inside layer reversely with default comparer
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	public static void ReverseUnsortedCells (int layerIndex) => Layers[layerIndex].ReverseUnsorted();


	/// <summary>
	/// Do not sort the unsorted cells inside this layer
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	public static void AbandonLayerSort (int layerIndex) => Layers[layerIndex].AbandonZSort();


	/// <summary>
	/// Get current cells count inside the using layer
	/// </summary>
	public static int GetUsedCellCount () => GetUsedCellCount(CurrentLayerIndex);
	/// <summary>
	/// Get current cells count inside given layer
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	public static int GetUsedCellCount (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Count : 0;

	/// <summary>
	/// Get total size of the layer
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	public static int GetLayerCapacity (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Cells.Length : 0;


	// Font
	/// <summary>
	/// Set current using font from ID
	/// </summary>
	public static void SetFontID (int id) {
		CurrentFontIndex = 0;
		if (id == 0) return;
		if (FontIdIndexMap.TryGetValue(id, out int index)) {
			// Got Index Value
			CurrentFontIndex = index;
		} else {
			// Find and Add Index into Pool
			index = 0;
			for (int i = 0; i < Game.Fonts.Count; i++) {
				if (Game.Fonts[i].ID == id) {
					index = i;
					break;
				}
			}
			CurrentFontIndex = index;
			FontIdIndexMap.Add(id, index);
		}
	}


	/// <summary>
	/// Set current using font from index
	/// </summary>
	public static void SetFontIndex (int index) => CurrentFontIndex = index;


	/// <summary>
	/// Reset internal cache for font id with index
	/// </summary>
	public static void ClearFontIndexIdMap () => FontIdIndexMap.Clear();


	/// <summary>
	/// Add/set internal cache for font id with index
	/// </summary>
	public static void OverrideFontIdAndIndex (int fontId, int fontIndex) => FontIdIndexMap[fontId] = fontIndex;


	// Draw
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (int globalID, IRect rect, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (int globalID, IRect rect, Color32 color, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(globalID, out var sprite, false) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (SpriteCode globalID, IRect rect, int z = int.MinValue) => Draw(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (SpriteCode globalID, IRect rect, Color32 color, int z = int.MinValue) => Draw(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(globalID.ID, out var sprite, false) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (AngeSprite sprite, IRect rect, int z = int.MinValue) => Draw(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (AngeSprite sprite, IRect rect, Color32 color, int z = int.MinValue) => Draw(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="Draw(AngeSprite, int, int, int, int, int, int, int, Color32, int, bool)"/>
	public static Cell Draw (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <summary>
	/// Draw a artwork sprite into using render layer
	/// </summary>
	/// <param name="sprite">Artwork sprite</param>
	/// <param name="globalID">Artwork sprite ID</param>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="pivotX">0 means "x" align with left edge. 1000 means "x" align with right edge.</param>
	/// <param name="pivotY">0 means "y" align with bottom edge. 1000 means "y" align with top edge.</param>
	/// <param name="rotation">Rotation in degree. 90 means facing right.</param>
	/// <param name="width">Size in global space</param>
	/// <param name="height">Size in global space</param>
	/// <param name="color">Color tint</param>
	/// <param name="z">Z value for sorting rendering cells</param>
	/// <param name="ignoreAttach">True if do not draw attaching sprite</param>
	/// <returns>Rendering cell for this sprite</returns>
	public static Cell Draw (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue, bool ignoreAttach = false) {

		if (!IsDrawing || sprite == null) return Cell.EMPTY;

		var layer = Layers[CurrentLayerIndex.Clamp(0, Layers.Length - 1)];
		if (Game.IsPausing && CurrentLayerIndex != RenderLayer.UI) return Cell.EMPTY;
		if (layer.FocusedCell < 0) return Cell.EMPTY;
		var cell = layer.Cells[layer.FocusedCell];

		// Original Size
		if (width == Const.ORIGINAL_SIZE) {
			width = sprite.GlobalWidth;
		} else if (width == Const.ORIGINAL_SIZE_NEGATAVE) {
			width = -sprite.GlobalWidth;
		}
		if (height == Const.ORIGINAL_SIZE) {
			height = sprite.GlobalHeight;
		} else if (height == Const.ORIGINAL_SIZE_NEGATAVE) {
			height = -sprite.GlobalHeight;
		}

		// Original Pivot
		pivotX = pivotX == Const.ORIGINAL_PIVOT ? sprite.PivotX : pivotX;
		pivotY = pivotY == Const.ORIGINAL_PIVOT ? sprite.PivotY : pivotY;

		// Cell
		cell.Sprite = sprite;
		cell.TextSprite = null;
		cell.SheetIndex = CurrentSheetIndex;
		cell.Order = layer.FocusedCell;
		cell.X = x;
		cell.Y = y;
		cell.Z = z != int.MinValue ? z : sprite.LocalZ;
		cell.Width = width;
		cell.Height = height;
		cell.Rotation = rotation;
		cell.PivotX = pivotX / 1000f;
		cell.PivotY = pivotY / 1000f;
		cell.Color = color;
		cell.BorderSide = Alignment.Full;
		cell.Shift = Int4.Zero;

		// Move Next
		layer.FocusedCell++;
		if (layer.FocusedCell >= layer.CellCount) {
			layer.FocusedCell = -1;
		}

		// Draw Attach
		if (!ignoreAttach && sprite.AttachedSprite != null) {
			var att = sprite.AttachedSprite;
			Draw(
				att, x, y, att.PivotX, att.PivotY, rotation, width, height,
				color, z: int.MinValue, ignoreAttach: true
			);
		}

		return cell;
	}


	/// <inheritdoc cref="DrawPixel(int, int, int, int, int, int, int, Color32, int)"/>
	public static Cell DrawPixel (IRect rect, int z = int.MinValue) => Draw(Const.PIXEL, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	/// <inheritdoc cref="DrawPixel(int, int, int, int, int, int, int, Color32, int)"/>
	public static Cell DrawPixel (IRect rect, Color32 color, int z = int.MinValue) => Draw(Const.PIXEL, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="DrawPixel(int, int, int, int, int, int, int, Color32, int)"/>
	public static Cell DrawPixel (int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(Const.PIXEL, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <summary>
	/// Draw a solid rectangle into using render layer
	/// </summary>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="pivotX">0 means "x" align with left edge. 1000 means "x" align with right edge.</param>
	/// <param name="pivotY">0 means "y" align with bottom edge. 1000 means "y" align with top edge.</param>
	/// <param name="rotation">Rotation in degree. 90 means facing right.</param>
	/// <param name="width">Size in global space</param>
	/// <param name="height">Size in global space</param>
	/// <param name="color">Color tint</param>
	/// <param name="z">Z value for sorting rendering cells</param>
	/// <returns>Rendering cell for this sprite</returns>
	public static Cell DrawPixel (int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(Const.PIXEL, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;


	// Char
	/// <inheritdoc cref="DrawChar(CharSprite, int, int, int, int, Color32)"/>
	public static Cell DrawChar (char c, int x, int y, int width, int height, Color32 color) {
		if (!IsDrawing) return Cell.EMPTY;
		if (!CharSpritePool.TryGetValue(new(c, CurrentFontIndex), out var tSprite)) return Cell.EMPTY;
		return DrawChar(tSprite, x, y, width, height, color);
	}
	/// <summary>
	/// Draw a text character into using render layer
	/// </summary>
	/// <param name="sprite"></param>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="width">Size in global space</param>
	/// <param name="height">Size in global space</param>
	/// <param name="color">Color tint</param>
	/// <returns>Rendering cell for this text character</returns>
	public static Cell DrawChar (CharSprite sprite, int x, int y, int width, int height, Color32 color) {

		if (!IsDrawing || sprite == null) return Cell.EMPTY;

		var layer = Layers[CurrentLayerIndex.Clamp(0, Layers.Length - 1)];
		if (layer.FocusedCell < 0) return Cell.EMPTY;

		var cell = layer.Cells[layer.FocusedCell];
		var uvOffset = sprite.Offset;

		cell.Z = 0;
		cell.Sprite = null;
		cell.TextSprite = sprite;
		cell.Order = layer.FocusedCell;
		cell.Rotation1000 = 0;
		cell.PivotX = 0;
		cell.PivotY = 0;
		cell.Color = color;
		cell.BorderSide = Alignment.Full;
		cell.Shift = Int4.Zero;
		cell.X = x + (int)(width * uvOffset.x);
		cell.Y = y + (int)(height * uvOffset.y);
		cell.Width = (int)(width * uvOffset.width);
		cell.Height = (int)(height * uvOffset.height);

		// Final
		layer.FocusedCell++;
		if (layer.FocusedCell >= layer.CellCount) {
			layer.FocusedCell = -1;
		}
		return cell;
	}


	// 9-Slice
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, IRect rect) => DrawSlice(globalID, rect, Color32.WHITE);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, IRect rect, Color32 color, int z = int.MinValue) => DrawSlice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => DrawSlice(globalID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		var border = TryGetSprite(globalID, out var sprite) ? sprite.GlobalBorder : default;
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			border.left, border.right,
			border.down, border.up, DEFAULT_PART_IGNORE,
			color, z
		);
	}
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z = int.MinValue) {
		TryGetSprite(globalID, out var sprite);
		return DrawSlice(
			sprite, x, y, pivotX, pivotY,
			rotation, width, height,
			borderL, borderR, borderD, borderU, partIgnore, color, z
		);
	}
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect) => DrawSlice(globalID.ID, rect, Color32.WHITE);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect, Color32 color, int z = int.MinValue) => DrawSlice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => DrawSlice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		var border = TryGetSprite(globalID.ID, out var sprite) ? sprite.GlobalBorder : default;
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			border.left, border.right,
			border.down, border.up, DEFAULT_PART_IGNORE,
			color, z
		);
	}
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z = int.MinValue) {
		TryGetSprite(globalID.ID, out var sprite);
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			borderL, borderR, borderD, borderU, partIgnore, color, z
		);
	}
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect) => DrawSlice(sprite, rect, Color32.WHITE);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect, Color32 color, int z = int.MinValue) => DrawSlice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => DrawSlice(sprite, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			sprite.GlobalBorder.left, sprite.GlobalBorder.right,
			sprite.GlobalBorder.down, sprite.GlobalBorder.up,
			DEFAULT_PART_IGNORE, color, z
		);
	}
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	/// <inheritdoc cref="DrawSlice(AngeSprite, int, int, int, int, int, int, int, int, int, int, int, bool[], Color32, int)"/>
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	/// <summary>
	/// Proportionally scale the sprite by dividing it into nine sections, protecting the corners and scaling/tiling the edges and center to maintain the image's integrity and prevent distortion.
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="globalID">Artwork sprite ID</param>
	/// <param name="sprite">Artwork sprite</param>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="pivotX">0 means "x" align with left edge. 1000 means "x" align with right edge.</param>
	/// <param name="pivotY">0 means "y" align with bottom edge. 1000 means "y" align with top edge.</param>
	/// <param name="rotation">Rotation in degree. 90 means facing right.</param>
	/// <param name="width">Size in global space</param>
	/// <param name="height">Size in global space</param>
	/// <param name="borderL">Padding left in global space</param>
	/// <param name="borderR">Padding right in global space</param>
	/// <param name="borderD">Padding down in global space</param>
	/// <param name="borderU">Padding up in global space</param>
	/// <param name="partIgnore">Which part should be ignored. Set to true to exclude that part.</param>
	/// <param name="color">Color tint</param>
	/// <param name="z">Z value for sorting rendering cells</param>
	/// <returns>9 Rendering cells for this sprite</returns>
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z) {

		SLICE_RESULT[0] = SLICE_RESULT[1] = SLICE_RESULT[2] = Cell.EMPTY;
		SLICE_RESULT[3] = SLICE_RESULT[4] = SLICE_RESULT[5] = Cell.EMPTY;
		SLICE_RESULT[6] = SLICE_RESULT[7] = SLICE_RESULT[8] = Cell.EMPTY;

		// Original Size
		if (sprite != null && width != 0 && height != 0) {
			if (width == Const.ORIGINAL_SIZE) {
				width = sprite.GlobalWidth;
			} else if (width == Const.ORIGINAL_SIZE_NEGATAVE) {
				width = -sprite.GlobalWidth;
			}
			if (height == Const.ORIGINAL_SIZE) {
				height = sprite.GlobalHeight;
			} else if (height == Const.ORIGINAL_SIZE_NEGATAVE) {
				height = -sprite.GlobalHeight;
			}
		} else return SLICE_RESULT;

		// Original Pivot
		pivotX = pivotX == Const.ORIGINAL_PIVOT ? sprite.PivotX : pivotX;
		pivotY = pivotY == Const.ORIGINAL_PIVOT ? sprite.PivotY : pivotY;

		bool flipX = width < 0;
		bool flipY = height < 0;
		width = width.Abs();
		height = height.Abs();

		borderL = borderL.Clamp(0, width);
		borderR = borderR.Clamp(0, width);
		borderD = borderD.Clamp(0, height);
		borderU = borderU.Clamp(0, height);

		bool hasL = borderL > 0;
		bool hasM = borderL + borderR < width;
		bool hasR = borderR > 0;
		int mWidth = width - borderL - borderR;
		int mHeight = height - borderD - borderU;

		float _px0 = width * pivotX / 1000f / borderL;
		float _px1 = (width * pivotX / 1000f - borderL) / mWidth;
		float _px2 = (borderR - width * (1000 - pivotX) / 1000f) / borderR;

		if (borderU > 0) {
			float _py = (borderU - height * (1000 - pivotY) / 1000f) / borderU;
			// TL
			if (hasL && !partIgnore[0]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, borderL, borderU, color, z
				);
				cell.BorderSide = Alignment.TopLeft;
				cell.PivotX = _px0;
				cell.PivotY = _py;
				SLICE_RESULT[0] = cell;
			}
			// TM
			if (hasM && !partIgnore[1]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, mWidth, borderU, color, z
				);
				cell.BorderSide = Alignment.TopMid;
				cell.PivotX = _px1;
				cell.PivotY = _py;
				SLICE_RESULT[1] = cell;
			}
			// TR
			if (hasR && !partIgnore[2]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, borderR, borderU, color, z
				);
				cell.PivotX = _px2;
				cell.PivotY = _py;
				cell.BorderSide = Alignment.TopRight;
				SLICE_RESULT[2] = cell;
			}
		}
		if (borderD + borderU < height) {
			float _py = (height * pivotY / 1000f - borderD) / mHeight;
			// ML
			if (hasL && !partIgnore[3]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, borderL, mHeight, color, z
				);
				cell.BorderSide = Alignment.MidLeft;
				cell.PivotX = _px0;
				cell.PivotY = _py;
				SLICE_RESULT[3] = cell;
			}
			// MM
			if (hasM && !partIgnore[4]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, mWidth, mHeight, color, z
				);
				cell.BorderSide = Alignment.MidMid;
				cell.PivotX = _px1;
				cell.PivotY = _py;
				SLICE_RESULT[4] = cell;
			}
			// MR
			if (hasR && !partIgnore[5]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, borderR, mHeight, color, z
				);
				cell.BorderSide = Alignment.MidRight;
				cell.PivotX = _px2;
				cell.PivotY = _py;
				SLICE_RESULT[5] = cell;
			}
		}
		if (borderD > 0) {
			float _py = height * pivotY / 1000f / borderD;
			// DL
			if (hasL && !partIgnore[6]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, borderL, borderD, color, z
				);
				cell.BorderSide = Alignment.BottomLeft;
				cell.PivotX = _px0;
				cell.PivotY = _py;
				SLICE_RESULT[6] = cell;
			}
			// DM
			if (hasM && !partIgnore[7]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, mWidth, borderD, color, z
				);
				cell.BorderSide = Alignment.BottomMid;
				cell.PivotX = _px1;
				cell.PivotY = _py;
				SLICE_RESULT[7] = cell;
			}
			// DR
			if (hasR && !partIgnore[8]) {
				var cell = Draw(
					sprite, x, y, 0, 0,
					rotation, borderR, borderD, color, z
				);
				cell.BorderSide = Alignment.BottomRight;
				cell.PivotX = _px2;
				cell.PivotY = _py;
				SLICE_RESULT[8] = cell;
			}
		}

		// Flip for Negative Size
		if (flipX) {
			foreach (var cell in SLICE_RESULT) cell.Width = -cell.Width;
		}
		if (flipY) {
			foreach (var cell in SLICE_RESULT) cell.Height = -cell.Height;
		}

		return SLICE_RESULT;
	}


	// Animation
	/// <summary>
	/// Get total duration in frame of an animation group
	/// </summary>
	public static int GetAnimationGroupDuration (int chainID) {
		if (!TryGetSpriteGroup(chainID, out var group) || !group.Animated) return 0;
		return CurrentSheet.GetSpriteAnimationDuration(group);
	}

	/// <summary>
	/// Get total duration in frame of an animation group
	/// </summary>
	public static int GetAnimationGroupDuration (SpriteGroup group) => CurrentSheet.GetSpriteAnimationDuration(group);


	/// <inheritdoc cref="DrawAnimation(SpriteGroup, int, int, int, int, int, int, int, int, int)"/>
	public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int z = int.MinValue) {
		if (!TryGetSpriteGroup(chainID, out var group) || !group.Animated) return Cell.EMPTY;
		return DrawAnimation(group, x, y, pivotX, pivotY, rotation, width, height, frame, z);
	}
	/// <summary>
	/// Draw the given animation into using render layer
	/// </summary>
	/// <param name="group"></param>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="pivotX">0 means "x" align with left edge. 1000 means "x" align with right edge.</param>
	/// <param name="pivotY">0 means "y" align with bottom edge. 1000 means "y" align with top edge.</param>
	/// <param name="rotation">Rotation in degree. 90 means facing right.</param>
	/// <param name="width">Size in global space</param>
	/// <param name="height">Size in global space</param>
	/// <param name="frame">Animation frame</param>
	/// <param name="z">Z value for sorting rendering cells</param>
	/// <returns>Rendering cell for this sprite</returns>
	public static Cell DrawAnimation (SpriteGroup group, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int z = int.MinValue) {
		if (CurrentSheet.TryGetSpriteFromAnimationFrame(group, frame, out var sprite)) {
			return Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, z);
		}
		return Cell.EMPTY;
	}


	// Sprite Data
	/// <summary>
	/// True if sprite with given ID is founded
	/// </summary>
	public static bool HasSprite (int globalID) => CurrentSheet.SpritePool.ContainsKey(globalID);


	/// <summary>
	/// True if sprite group with given ID is founded
	/// </summary>
	public static bool HasSpriteGroup (int groupID) => CurrentSheet.GroupPool.ContainsKey(groupID);
	/// <summary>
	/// True if sprite group with given ID is founded
	/// </summary>
	/// <param name="groupID"></param>
	/// <param name="groupLength"></param>
	public static bool HasSpriteGroup (int groupID, out int groupLength) {
		if (CurrentSheet.GroupPool.TryGetValue(groupID, out var values)) {
			groupLength = values.Count;
			return true;
		} else {
			groupLength = 0;
			return false;
		}
	}


	/// <summary>
	/// Get instance of sprite
	/// </summary>
	/// <param name="globalID"></param>
	/// <param name="sprite"></param>
	/// <param name="ignoreAnimation">True if ignore animation group with "globalID" as chainID</param>
	/// <returns>True if sprite founded</returns>
	public static bool TryGetSprite (int globalID, out AngeSprite sprite, bool ignoreAnimation = true) {
		var sheet = CurrentSheet;
		if (sheet.SpritePool.TryGetValue(globalID, out sprite)) return true;
		if (!ignoreAnimation && sheet.GroupPool.TryGetValue(globalID, out var group) && group.Animated) {
			return sheet.TryGetSpriteFromAnimationFrame(group, Game.GlobalFrame, out sprite);
		}
		sprite = null;
		return false;
	}


	/// <summary>
	/// Get instance of animation group
	/// </summary>
	/// <param name="groupID"></param>
	/// <param name="group"></param>
	/// <returns>True if animation group founded</returns>
	public static bool TryGetAnimationGroup (int groupID, out SpriteGroup group) => CurrentSheet.GroupPool.TryGetValue(groupID, out group) && group.Animated;


	/// <summary>
	/// Get instance of sprite group
	/// </summary>
	/// <param name="groupID"></param>
	/// <param name="group"></param>
	/// <returns>True if sprite group founded</returns>
	public static bool TryGetSpriteGroup (int groupID, out SpriteGroup group) => CurrentSheet.GroupPool.TryGetValue(groupID, out group);


	/// <summary>
	/// Get sprite instance from given group ID and index
	/// </summary>
	/// <param name="groupID"></param>
	/// <param name="index"></param>
	/// <param name="sprite"></param>
	/// <param name="loopIndex">True if the index loop</param>
	/// <param name="clampIndex">True if clamp the index when out of range</param>
	/// <param name="ignoreAnimatedWhenFailback">True if don't use animated sprite when returning the failback sprite</param>
	/// <returns>True if sprite founded</returns>
	public static bool TryGetSpriteFromGroup (int groupID, int index, out AngeSprite sprite, bool loopIndex = true, bool clampIndex = true, bool ignoreAnimatedWhenFailback = true) {
		if (CurrentSheet.GroupPool.TryGetValue(groupID, out var group)) {
			if (loopIndex) index = index.UMod(group.Count);
			if (clampIndex) index = index.Clamp(0, group.Count - 1);
			if (index >= 0 && index < group.Count) {
				sprite = group.Sprites[index];
				return sprite != null;
			} else {
				sprite = null;
				return false;
			}
		} else {
			return TryGetSprite(groupID, out sprite, ignoreAnimatedWhenFailback);
		}
	}


	/// <summary>
	/// Get sprite for rendering UI/icon/gizmos
	/// </summary>
	/// <param name="artworkID"></param>
	/// <param name="sprite"></param>
	/// <returns>True if sprite founded</returns>
	public static bool TryGetSpriteForGizmos (int artworkID, out AngeSprite sprite) => TryGetSprite(artworkID, out sprite, true) || TryGetSpriteFromGroup(artworkID, 0, out sprite);


	// Clamp
	/// <summary>
	/// Clamp cells in using layer inside rect range
	/// </summary>
	/// <param name="rect">Target range in global space</param>
	/// <param name="startIndex"></param>
	/// <param name="endIndex">(excluded)</param>
	public static void ClampCells (IRect rect, int startIndex, int endIndex = -1) {
		if (endIndex < 0) endIndex = GetUsedCellCount(CurrentLayerIndex);
		FrameworkUtil.ClampCells(Layers[CurrentLayerIndex].Cells, rect, startIndex, endIndex);
	}

	/// <summary>
	/// Clamp cells for given layer inside rect range
	/// </summary>
	/// <param name="layerIndex">Use RenderLayer.XXX to get this value</param>
	/// <param name="rect">Target range in global space</param>
	/// <param name="startIndex"></param>
	/// <param name="endIndex">(excluded)</param>
	public static void ClampCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
		if (endIndex < 0) endIndex = GetUsedCellCount(layerIndex);
		FrameworkUtil.ClampCells(Layers[layerIndex].Cells, rect, startIndex, endIndex);
	}

	/// <summary>
	/// Clamp cells inside rect range
	/// </summary>
	/// <param name="cells"></param>
	/// <param name="rect">Target range in global space</param>
	/// <param name="startIndex"></param>
	/// <param name="endIndex">(excluded)</param>
	public static void ClampCells (Cell[] cells, IRect rect, int startIndex = 0, int endIndex = -1) => FrameworkUtil.ClampCells(cells, rect, startIndex, endIndex);


	// Layer Access
	/// <summary>
	/// Get cells inside using layer
	/// </summary>
	/// <param name="cells"></param>
	/// <param name="count"></param>
	/// <returns>True if cells founded</returns>
	public static bool GetCells (out Span<Cell> cells, out int count) => GetCells(CurrentLayerIndex, out cells, out count);

	/// <summary>
	/// Get cells inside given layer
	/// </summary>
	/// <param name="layer">Use RenderLayer.XXX to get this value</param>
	/// <param name="cells"></param>
	/// <param name="count"></param>
	/// <returns>True if cells founded</returns>
	public static bool GetCells (int layer, out Span<Cell> cells, out int count) {
		if (layer >= 0 && layer < LayerCount) {
			var item = Layers[layer];
			count = item.Count;
			cells = item.Cells;
			return true;
		} else {
			count = 0;
			cells = null;
			return false;
		}
	}


	/// <summary>
	/// Get current color tint for given layer
	/// </summary>
	/// <param name="layer">Use RenderLayer.XXX to get this value</param>
	public static Color32 GetLayerTint (int layer) => Layers[layer].LayerTint;

	/// <summary>
	/// Set current color tint for given layer
	/// </summary>
	/// <param name="layer">Use RenderLayer.XXX to get this value</param>
	/// <param name="tint"></param>
	public static void SetLayerTint (int layer, Color32 tint) => Layers[layer].LayerTint = tint;

	/// <summary>
	/// Make current color tint multiply given value for given layer
	/// </summary>
	/// <param name="layer">Use RenderLayer.XXX to get this value</param>
	/// <param name="tint"></param>
	public static void MultLayerTint (int layer, Color32 tint) => Layers[layer].LayerTint *= tint;


	// Internal
	/// <summary>
	/// Require given text character from internal caching
	/// </summary>
	public static bool RequireCharForPool (char c, out CharSprite charSprite) => RequireCharForPool(c, CurrentFontIndex, out charSprite);

	/// <summary>
	/// Require given text character from internal caching
	/// </summary>
	public static bool RequireCharForPool (char c, int fontIndex, out CharSprite charSprite) {
		if (CharSpritePool.TryGetValue(new Int2(c, fontIndex), out var textSprite)) {
			// Get Exists
			charSprite = textSprite;
		} else if (Game.GetCharSprite(fontIndex, c, out charSprite)) {
			// Require Char from Font
			CharSpritePool.Add(new(c, fontIndex), charSprite);
		}
		return charSprite != null;
	}


	#endregion




}
