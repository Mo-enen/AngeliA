using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class Renderer {




	#region --- SUB ---


	public class CellComparer : IComparer<Cell> {
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
		public bool UiLayer;
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
	private static readonly bool[] DEFAULT_PART_IGNORE = new bool[9] { false, false, false, false, false, false, false, false, false, };
	private static readonly int[] DEFAULT_CAPACITY = new int[RenderLayer.COUNT] { 256, 8192, 4096, 16384, 256, 128, 128, 8192, };
	private static readonly Cell[] SLICE_RESULT = new Cell[9];

	// Event
	public static event Action OnSheetLoaded;

	// Api
	public static IRect CameraRect { get; private set; } = new IRect(0, 0, 1, 1);
	public static FRect CameraRange { get; private set; } = new(0, 0, 1f, 1f);
	public static IRect ScreenRenderRect { get; private set; }
	public static float CameraRestrictionRate { get; private set; } = 1f;
	public static int LayerCount => Layers.Length;
	public static int SpriteCount => MainSheet.Sprites.Count;
	public static int GroupCount => MainSheet.Groups.Count;
	public static int CurrentSheetIndex {
		get => _CurrentSheetIndex;
		set {
			if (value != _CurrentSheetIndex) {
				_CurrentSheetIndex = value;
				CurrentSheet = CurrentSheetIndex < 0 || CurrentSheetIndex >= AltSheets.Count ? MainSheet : AltSheets[CurrentSheetIndex];
			}
		}
	}
	public static int CurrentLayerIndex { get; private set; } = 0;
	public static int CurrentFontIndex { get; private set; } = 0;
	public static int AltSheetCount => AltSheets.Count;
	public static Sheet CurrentSheet { get; private set; }
	public static bool IsReady { get; private set; } = false;

	// Data
	private static readonly Sheet MainSheet = new(ignoreTextureAndPixels: Game.IgnoreArtworkPixels);
	private static readonly List<Sheet> AltSheets = new();
	private static readonly Layer[] Layers = new Layer[RenderLayer.COUNT];
	private static readonly Dictionary<Int2, CharSprite> CharSpritePool = new();
	private static int _CurrentSheetIndex = -1;
	private static bool IsDrawing = false;
	private static long MainSheetFileModifyDate = 0;
	private static string MainSheetFilePath = "";


	#endregion




	#region --- MSG ---


	// Init
	[OnGameInitialize(-4096)]
	internal static void Initialize () {

		Util.LinkEventWithAttribute<OnSheetReloadAttribute>(typeof(Renderer), nameof(OnSheetLoaded));

		// Create Layers
		var capacities = new int[RenderLayer.COUNT];
		DEFAULT_CAPACITY.CopyTo(capacities, 0);
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<RenderLayerCapacityAttribute>()) {
			if (att.Layer < 0 || att.Layer >= RenderLayer.COUNT) continue;
			capacities[att.Layer] = att.Capacity;
		}
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			int capacity = capacities[i];
			int order = i;
			bool uiLayer = i == RenderLayer.UI;
			Layers[i] = new Layer {
				Cells = new Cell[capacity].FillWithNewValue(),
				CellCount = capacity,
				FocusedCell = 0,
				PrevCellCount = 0,
				SortedIndex = 0,
				SortingOrder = order,
				UiLayer = uiLayer
			};
		}

		// Load Sheet
		LoadMainSheet();
		CurrentSheet = MainSheet;

		// End
		IsReady = true;

	}


	[OnGameFocused]
	internal static void OnGameFocused () {
#if DEBUG
		// Reload Main Sheet on Changed
		long date = Util.GetFileModifyDate(MainSheetFilePath);
		if (date > MainSheetFileModifyDate) {
			LoadMainSheet();
			Util.DeleteFolder(Universe.BuiltIn.CharacterRenderingConfigRoot);
			PoseCharacter.ReloadRenderingConfigPoolFromFileAndSheet();
		}
#endif
	}


	// Update
	[OnGameUpdate(-2048)]
	internal static void UpdateCameraRect () {

		var viewRect = Stage.ViewRect;

		// Ratio
		float ratio = (float)Game.ScreenWidth / Game.ScreenHeight;
		float maxRatio = Game.IsToolApplication ?
			float.MaxValue - 1f :
			Const.VIEW_RATIO / 1000f;
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
	internal static void FrameUpdate () {
		IsDrawing = false;
		for (int i = 0; i < Layers.Length; i++) {
			try {
				var layer = Layers[i];
				if (!layer.UiLayer) layer.ZSort();
				int prevCount = layer.Count;
				if (Game.PauselessFrame < 4) continue;
				Game.OnLayerUpdate(i, layer.UiLayer, layer.Cells, layer.Count);
				layer.PrevCellCount = prevCount;
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () => MainSheet.Clear();


	[OnGameUpdate(-512)]
	public static void BeginDraw () {
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
			if (Game.IsPlaying || layer.UiLayer) {
				layer.FocusedCell = 0;
				layer.SortedIndex = 0;
			}
		}
	}


	[OnGameUpdatePauseless(-512)]
	public static void UpdatePausing () {
		if (Game.IsPlaying) return;
		BeginDraw();
	}


	#endregion




	#region --- API ---


	public static void ClearCharSpritePool () {
		foreach (var (_, sprite) in CharSpritePool) {
			Game.UnloadTexture(sprite.Texture);
		}
		CharSpritePool.Clear();
	}


	// Sheet
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


	public static void LoadMainSheet () {

		string path = Universe.BuiltIn.SheetPath;

		// Load Sheet
		MainSheetFileModifyDate = 0;
		MainSheetFilePath = "";
		bool loaded = MainSheet.LoadFromDisk(path);
		if (loaded) {
			MainSheetFilePath = path;
			MainSheetFileModifyDate = Util.GetFileModifyDate(path);
		}

		// Event
		OnSheetLoaded?.Invoke();
	}


	public static int AddAltSheet (Sheet sheet) {
		AltSheets.Add(sheet);
		return AltSheets.Count - 1;
	}


	public static void RemoveAltSheet (int index) => AltSheets.RemoveAt(index);


	public static Sheet GetAltSheet (int index) => AltSheets[index];


	// Layer
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

	public static void ResetLayer (int layerIndex) {
		var layer = Layers[layerIndex];
		layer.FocusedCell = 0;
		layer.SortedIndex = 0;
	}

	public static void SortLayer (int layerIndex) => Layers[layerIndex].ZSort();
	public static void ReverseUnsortedCells (int layerIndex) => Layers[layerIndex].ReverseUnsorted();
	public static void AbandonLayerSort (int layerIndex) => Layers[layerIndex].AbandonZSort();


	public static int GetUsedCellCount () => GetUsedCellCount(CurrentLayerIndex);
	public static int GetUsedCellCount (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Count : 0;

	public static int GetLayerCapacity (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Cells.Length : 0;


	public static void SetFontIndex (int newIndex) => CurrentFontIndex = newIndex;


	// Draw
	public static Cell Draw (int globalID, IRect rect, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	public static Cell Draw (int globalID, IRect rect, Color32 color, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(globalID, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;
	public static Cell Draw (SpriteCode globalID, IRect rect, int z = int.MinValue) => Draw(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	public static Cell Draw (SpriteCode globalID, IRect rect, Color32 color, int z = int.MinValue) => Draw(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell Draw (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell Draw (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(globalID.ID, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;
	public static Cell Draw (AngeSprite sprite, IRect rect, int z = int.MinValue) => Draw(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	public static Cell Draw (AngeSprite sprite, IRect rect, Color32 color, int z = int.MinValue) => Draw(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell Draw (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell Draw (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {

		if (!IsDrawing || sprite == null) return Cell.EMPTY;

		var layer = Layers[CurrentLayerIndex.Clamp(0, Layers.Length - 1)];
		if (Game.IsPausing && !layer.UiLayer) return Cell.EMPTY;
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

		// Cell
		cell.Sprite = sprite;
		cell.TextSprite = null;
		cell.SheetIndex = CurrentSheetIndex;
		cell.Order = layer.FocusedCell;
		cell.X = x;
		cell.Y = y;
		cell.Z = z != int.MinValue ? z : sprite.SortingZ;
		cell.Width = width;
		cell.Height = height;
		cell.Rotation = rotation;
		cell.PivotX = pivotX / 1000f;
		cell.PivotY = pivotY / 1000f;
		cell.Color = color;
		cell.BorderSide = Alignment.Full;
		cell.Shift = Int4.zero;

		// Move Next
		layer.FocusedCell++;
		if (layer.FocusedCell >= layer.CellCount) {
			layer.FocusedCell = -1;
		}
		return cell;
	}

	public static Cell DrawPixel (IRect rect, int z = int.MinValue) => Draw(Const.PIXEL, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	public static Cell DrawPixel (IRect rect, Color32 color, int z = int.MinValue) => Draw(Const.PIXEL, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell DrawPixel (int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(Const.PIXEL, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell DrawPixel (int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(Const.PIXEL, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;


	// Char
	public static Cell DrawChar (char c, int x, int y, int width, int height, Color32 color) {
		if (!IsDrawing) return Cell.EMPTY;
		if (!CharSpritePool.TryGetValue(new(c, CurrentFontIndex), out var tSprite)) return Cell.EMPTY;
		return DrawChar(tSprite, x, y, width, height, color);
	}
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
		cell.Shift = Int4.zero;
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
	public static Cell[] DrawSlice (int globalID, IRect rect) => DrawSlice(globalID, rect, Color32.WHITE);
	public static Cell[] DrawSlice (int globalID, IRect rect, Color32 color, int z = int.MinValue) => DrawSlice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell[] DrawSlice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] DrawSlice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => DrawSlice(globalID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		var border = TryGetSprite(globalID, out var sprite) ? sprite.GlobalBorder : default;
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			border.left, border.right,
			border.down, border.up, DEFAULT_PART_IGNORE,
			color, z
		);
	}
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	public static Cell[] DrawSlice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z = int.MinValue) {
		TryGetSprite(globalID, out var sprite);
		return DrawSlice(
			sprite, x, y, pivotX, pivotY,
			rotation, width, height,
			borderL, borderR, borderD, borderU, partIgnore, color, z
		);
	}
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect) => DrawSlice(globalID.ID, rect, Color32.WHITE);
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect, Color32 color, int z = int.MinValue) => DrawSlice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] DrawSlice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => DrawSlice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		var border = TryGetSprite(globalID.ID, out var sprite) ? sprite.GlobalBorder : default;
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			border.left, border.right,
			border.down, border.up, DEFAULT_PART_IGNORE,
			color, z
		);
	}
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	public static Cell[] DrawSlice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z = int.MinValue) {
		TryGetSprite(globalID.ID, out var sprite);
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			borderL, borderR, borderD, borderU, partIgnore, color, z
		);
	}
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect) => DrawSlice(sprite, rect, Color32.WHITE);
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect, Color32 color, int z = int.MinValue) => DrawSlice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] DrawSlice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => DrawSlice(sprite, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		return DrawSlice(
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			sprite.GlobalBorder.left, sprite.GlobalBorder.right,
			sprite.GlobalBorder.down, sprite.GlobalBorder.up,
			DEFAULT_PART_IGNORE, color, z
		);
	}
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => DrawSlice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] DrawSlice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => DrawSlice(
		sprite, x, y, pivotX, pivotY, rotation, width, height,
		borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z
	);
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


	// Sprite Data
	public static bool TryGetSprite (int globalID, out AngeSprite sprite, bool ignoreAnimation = false) {
		var sheet = CurrentSheet;
		if (sheet.SpritePool.TryGetValue(globalID, out sprite)) return true;
		if (!ignoreAnimation && sheet.GroupPool.TryGetValue(globalID, out var group) && group.Animated) {
			int id = sheet.GetSpriteIdFromAnimationFrame(group, Game.GlobalFrame);
			return sheet.SpritePool.TryGetValue(id, out sprite);
		}
		sprite = null;
		return false;
	}


	public static bool HasSpriteGroup (int groupID) => CurrentSheet.GroupPool.ContainsKey(groupID);


	public static bool HasSpriteGroup (int groupID, out int groupLength) {
		if (CurrentSheet.GroupPool.TryGetValue(groupID, out var values)) {
			groupLength = values.Count;
			return true;
		} else {
			groupLength = 0;
			return false;
		}
	}


	public static bool TryGetSpriteGroup (int groupID, out SpriteGroup group) => CurrentSheet.GroupPool.TryGetValue(groupID, out group);


	public static bool TryGetSpriteFromGroup (int groupID, int index, out AngeSprite sprite, bool loopIndex = true, bool clampIndex = true, bool ignoreAnimatedWhenFailback = true) {
		if (CurrentSheet.GroupPool.TryGetValue(groupID, out var group)) {
			if (loopIndex) index = index.UMod(group.Count);
			if (clampIndex) index = index.Clamp(0, group.Count - 1);
			if (index >= 0 && index < group.Count) {
				return TryGetSprite(group.SpriteIDs[index], out sprite, ignoreAnimatedWhenFailback);
			} else {
				sprite = null;
				return false;
			}
		} else {
			return TryGetSprite(groupID, out sprite, ignoreAnimatedWhenFailback);
		}
	}


	public static bool HasSprite (int globalID) => CurrentSheet.SpritePool.ContainsKey(globalID);


	public static AngeSprite GetSpriteAt (int index) {
		var sheet = CurrentSheet;
		return index >= 0 && index < sheet.Sprites.Count ? sheet.Sprites[index] : null;
	}


	public static SpriteGroup GetGroupAt (int index) {
		var sheet = CurrentSheet;
		return index >= 0 && index < sheet.Groups.Count ? sheet.Groups[index] : null;
	}


	// Clamp
	public static void ClampCells (IRect rect, int startIndex, int endIndex = -1) {
		if (endIndex < 0) endIndex = GetUsedCellCount(CurrentLayerIndex);
		Util.ClampCells(Layers[CurrentLayerIndex].Cells, rect, startIndex, endIndex);
	}
	public static void ClampCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
		if (endIndex < 0) endIndex = GetUsedCellCount(layerIndex);
		Util.ClampCells(Layers[layerIndex].Cells, rect, startIndex, endIndex);
	}
	public static void ClampCells (Cell[] cells, IRect rect) => Util.ClampCells(cells, rect, 0, cells.Length);


	// Layer Access
	public static bool GetCells (out Cell[] cells, out int count) => GetCells(CurrentLayerIndex, out cells, out count);
	public static bool GetCells (int layer, out Cell[] cells, out int count) {
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


	// Internal
	public static bool RequireCharForPool (char c, out CharSprite charSprite) => RequireCharForPool(c, CurrentFontIndex, out charSprite);
	public static bool RequireCharForPool (char c, int fontIndex, out CharSprite charSprite) {
		if (CharSpritePool.TryGetValue(new(c, fontIndex), out var textSprite)) {
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
