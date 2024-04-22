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

	private class TextLayer : Layer {
		public readonly Dictionary<int, CharSprite> TextIDMap = new();
	}


	private class Layer {
		public int Count => Util.Min(Cells.Length, FocusedCell >= 0 ? FocusedCell : Cells.Length);
		public Cell[] Cells;
		public string Name;
		public int CellCount;
		public int FocusedCell;
		public int PrevCellCount;
		public int SortedIndex;
		public int SortingOrder;
		public bool UiLayer;
		public void ZSort () {
			if (SortedIndex < Count - 1) {
				Util.QuickSort(Cells, SortedIndex, Count - 1, CellComparer.Instance);
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
	public static readonly int[] DEFAULT_CAPACITY = new int[RenderLayer.COUNT] { 256, 8192, 4096, 16384, 256, 128, 128, 4096, };

	// Event
	public static event System.Action OnSheetLoaded;

	// Api
	public static IRect ViewRect { get; private set; } = new IRect(0, 0, 1, 1);
	public static IRect CameraRect { get; private set; } = new IRect(0, 0, 1, 1);
	public static float CameraRestrictionRate { get; private set; } = 1f;
	public static int LastDrawnID { get; private set; } = 0;
	public static int LayerCount => Layers.Length;
	public static int SpriteCount => Sheet.Sprites.Count;
	public static int GroupCount => Sheet.Groups.Count;
	public static int TextLayerCount => TextLayers.Length;
	public static int CurrentLayerIndex { get; private set; } = 0;
	public static int CurrentTextLayerIndex { get; private set; } = 0;
	public static bool TextReady => TextLayers.Length > 0;
	public static int CurrentSheetIndex { get; set; } = -1;
	public static int AltSheetCount => AltSheets.Count;
	public static Sheet CurrentSheet => CurrentSheetIndex < 0 || CurrentSheetIndex >= AltSheets.Count ? Sheet : AltSheets[CurrentSheetIndex];

	// Data
	private static readonly Sheet Sheet = new();
	private static readonly List<Sheet> AltSheets = new();
	private static readonly Layer[] Layers = new Layer[RenderLayer.COUNT];
	private static TextLayer[] TextLayers = System.Array.Empty<TextLayer>();
	private static bool IsDrawing = false;


	#endregion




	#region --- MSG ---


	// Init
	[OnGameInitialize(-4096)]
	internal static void Initialize () {

		// Create Layers
		var capacities = new int[RenderLayer.COUNT];
		DEFAULT_CAPACITY.CopyTo(capacities, 0);
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<RenderLayerCapacityAttribute>()) {
			if (att.Layer < 0 || att.Layer >= RenderLayer.COUNT) continue;
			capacities[att.Layer] = att.Capacity;
		}
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			int capacity = capacities[i];
			string name = RenderLayer.NAMES[i];
			int order = i;
			bool uiLayer = i == RenderLayer.UI;
			Layers[i] = CreateLayer(name, uiLayer, order, capacity, textLayer: false);
			Game.OnRenderingLayerCreated(i, name, order, capacity);
		}

		// Text Layer
		int textLayerCount = Game.TextLayerCount;
		const int TEXT_CAPACITY = 2048;
		TextLayers = new TextLayer[textLayerCount];
		for (int i = 0; i < textLayerCount; i++) {
			string name = Game.GetTextLayerName(i);
			int sortingOrder = Layers.Length + i;
			TextLayers[i] = CreateLayer(
				name,
				uiLayer: true,
				sortingOrder,
				TEXT_CAPACITY,
				textLayer: true
			) as TextLayer;
			Game.OnTextLayerCreated(i, name, sortingOrder, TEXT_CAPACITY);
		}

		// Load Sheet
		LoadSheet(UniverseSystem.BuiltInUniverse);

		// Func
		static Layer CreateLayer (string name, bool uiLayer, int sortingOrder, int renderCapacity, bool textLayer) {
			var layer = textLayer ? new TextLayer() : new Layer();
			layer.Name = name;
			layer.Cells = new Cell[renderCapacity].FillWithNewValue();
			layer.CellCount = renderCapacity;
			layer.FocusedCell = 0;
			layer.PrevCellCount = 0;
			layer.SortedIndex = 0;
			layer.SortingOrder = sortingOrder;
			layer.UiLayer = uiLayer;
			return layer;
		}
	}


	[OnUniverseOpen]
	internal static void OnUniverseOpen () {
		if (Game.GlobalFrame == 0) return;
		LoadSheet(UniverseSystem.CurrentUniverse);
	}


	// Update
	[OnGameUpdate(-2048)]
	internal static void CameraUpdate () {

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
		if (Game.CameraScreenLocacion.NotAlmost(rect)) {
			Game.CameraScreenLocacion = rect;
		}

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
		ViewRect = viewRect;

	}


	[OnGameUpdatePauseless(-2048)]
	internal static void PausingUpdate () {
		if (Game.IsPlaying) return;
		CameraUpdate();
	}


	[OnGameUpdatePauseless(32)]
	internal static void FrameUpdate () {
		IsDrawing = false;
		// Cell
		try {
			for (int i = 0; i < Layers.Length; i++) {
				var layer = Layers[i];
				if (!layer.UiLayer) layer.ZSort();
				int prevCount = layer.Count;
				Game.OnLayerUpdate(i, layer.UiLayer, false, layer.Cells, layer.Count);
				layer.PrevCellCount = prevCount;
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
		// Text
		try {
			for (int i = 0; i < TextLayers.Length; i++) {
				var layer = TextLayers[i];
				layer.ZSort();
				int prevCount = layer.Count;
				Game.OnLayerUpdate(i, layer.UiLayer, true, layer.Cells, layer.Count);
				layer.PrevCellCount = prevCount;
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () => Sheet.Clear();


	[OnGameUpdate(-512)]
	public static void BeginDraw () {
		IsDrawing = true;
		Cell.EMPTY.Sprite = null;
		Cell.EMPTY.TextSprite = null;
		Cell.EMPTY.Color = Color32.CLEAR;
		SetLayerToDefault();
		CurrentSheetIndex = -1;
		for (int i = 0; i < Layers.Length; i++) {
			var layer = Layers[i];
			if (Game.IsPlaying || layer.UiLayer) {
				layer.FocusedCell = 0;
				layer.SortedIndex = 0;
			}
		}
		for (int i = 0; i < TextLayers.Length; i++) {
			var tLayer = TextLayers[i];
			tLayer.FocusedCell = 0;
			tLayer.SortedIndex = 0;
		}
	}


	[OnGameUpdatePauseless(-512)]
	public static void UpdatePausing () {
		if (Game.IsPlaying) return;
		BeginDraw();
	}


	#endregion




	#region --- API ---


	// Sheet
	public static bool TryGetTextureFromSheet<T> (int spriteID, int sheetIndex, out T texture) {
		var sheet = sheetIndex < 0 || sheetIndex >= AltSheets.Count ? Sheet : AltSheets[sheetIndex];
		if (sheet.TexturePool.TryGetValue(spriteID, out object textureObj) && textureObj is T result) {
			texture = result;
			return true;
		} else {
			texture = default;
			return false;
		}
	}


	public static void LoadSheet (Universe project) {

		// Artwork >> Sheet
		SheetUtil.RecreateSheetIfArtworkModified(project.SheetPath, project.ArtworkRoot);

		// Load Sheet
		if (!Sheet.LoadFromDisk(project.SheetPath) && project != UniverseSystem.BuiltInUniverse) {
			Sheet.LoadFromDisk(UniverseSystem.BuiltInUniverse.SheetPath);
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
	public static void SetTextLayer (int index) => CurrentTextLayerIndex = index.Clamp(0, TextLayers.Length - 1);
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

	public static string GetLayerName (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Name : "";
	public static string GetTextLayerName (int layerIndex) => layerIndex >= 0 && layerIndex < TextLayers.Length ? TextLayers[layerIndex].Name : "";


	public static int GetUsedCellCount () => GetUsedCellCount(CurrentLayerIndex);
	public static int GetUsedCellCount (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Count : 0;
	public static int GetTextUsedCellCount () => GetTextUsedCellCount(CurrentTextLayerIndex);
	public static int GetTextUsedCellCount (int layerIndex) => layerIndex >= 0 && layerIndex < TextLayers.Length ? TextLayers[layerIndex].Count : 0;


	public static int GetLayerCapacity (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Cells.Length : 0;
	public static int GetTextLayerCapacity () => GetTextLayerCapacity(CurrentTextLayerIndex);
	public static int GetTextLayerCapacity (int layerIndex) => layerIndex >= 0 && layerIndex < TextLayers.Length ? TextLayers[layerIndex].Cells.Length : 0;


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
		LastDrawnID = sprite.ID;
		return cell;
	}

	public static Cell DrawPixel (IRect rect, int z = int.MinValue) => Draw(Const.PIXEL, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Color32.WHITE, z);
	public static Cell DrawPixel (IRect rect, Color32 color, int z = int.MinValue) => Draw(Const.PIXEL, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell DrawPixel (int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(Const.PIXEL, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell DrawPixel (int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) => TryGetSprite(Const.PIXEL, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : Cell.EMPTY;


	public static Cell DrawChar (char c, int x, int y, int width, int height, Color32 color) {
		if (!IsDrawing || !TextReady) return Cell.EMPTY;
		var layer = TextLayers[CurrentTextLayerIndex.Clamp(0, TextLayers.Length - 1)];
		if (!layer.TextIDMap.TryGetValue(c, out var tSprite)) return Cell.EMPTY;
		return DrawChar(tSprite, x, y, width, height, color);
	}

	public static Cell DrawChar (CharSprite sprite, int x, int y, int width, int height, Color32 color) {

		if (!IsDrawing || !TextReady || sprite == null) return Cell.EMPTY;

		var layer = TextLayers[CurrentTextLayerIndex.Clamp(0, TextLayers.Length - 1)];
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
		LastDrawnID = sprite.Char;
		return cell;
	}


	public static Cell[] Draw_9Slice (int globalID, IRect rect) => Draw_9Slice(globalID, rect, Color32.WHITE);
	public static Cell[] Draw_9Slice (int globalID, IRect rect, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell[] Draw_9Slice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		var border = TryGetSprite(globalID, out var sprite) ? sprite.GlobalBorder : default;
		return Util.NineSlice(
			Draw,
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			border.left, border.right,
			border.down, border.up, DEFAULT_PART_IGNORE,
			color, z
		);
	}
	public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z = int.MinValue) {
		TryGetSprite(globalID, out var sprite);
		return Util.NineSlice(
			Draw, sprite, x, y, pivotX, pivotY,
			rotation, width, height,
			borderL, borderR, borderD, borderU, partIgnore, color, z
		);
	}
	public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect) => Draw_9Slice(globalID.ID, rect, Color32.WHITE);
	public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		var border = TryGetSprite(globalID.ID, out var sprite) ? sprite.GlobalBorder : default;
		return Util.NineSlice(
			Draw,
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			border.left, border.right,
			border.down, border.up, DEFAULT_PART_IGNORE,
			color, z
		);
	}
	public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
	public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Color32 color, int z = int.MinValue) {
		TryGetSprite(globalID.ID, out var sprite);
		return Util.NineSlice(
			Draw,
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			borderL, borderR, borderD, borderU, partIgnore, color, z
		);
	}
	public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect) => Draw_9Slice(sprite, rect, Color32.WHITE);
	public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect, Color32 color, int z = int.MinValue) => Draw_9Slice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
	public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => Draw_9Slice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
	public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {
		return Util.NineSlice(
			Draw,
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			sprite.GlobalBorder.left, sprite.GlobalBorder.right,
			sprite.GlobalBorder.down, sprite.GlobalBorder.up,
			DEFAULT_PART_IGNORE, color, z
		);
	}
	public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Color32.WHITE, z);
	public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) {
		return Util.NineSlice(
			Draw,
			sprite, x, y, pivotX, pivotY, rotation, width, height,
			borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z
		);
	}

	public static Cell DrawAnimation (int chainID, IRect globalRect, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, Color32.WHITE, loopStart);
	public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, Color32.WHITE, loopStart);
	public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, Color32.WHITE, loopStart);
	public static Cell DrawAnimation (int chainID, IRect globalRect, int frame, Color32 color, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, color, loopStart);
	public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, Color32 color, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, color, loopStart);
	public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, Color32 color, int loopStart = int.MinValue) {
		if (!TryGetSpriteGroup(chainID, out var group) || !group.Animated) return Cell.EMPTY;
		int id = CurrentSheet.GetSpriteIdFromAnimationFrame(group, frame, loopStart);
		return Draw(id, x, y, pivotX, pivotY, rotation, width, height, color);
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
		} else return TryGetSprite(groupID, out sprite, ignoreAnimatedWhenFailback);
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
	public static void ClampTextCells (IRect rect, int startIndex, int endIndex = -1) {
		if (CurrentTextLayerIndex < 0 || CurrentTextLayerIndex >= TextLayers.Length) return;
		if (endIndex < 0) endIndex = GetTextUsedCellCount(CurrentTextLayerIndex);
		Util.ClampCells(TextLayers[CurrentTextLayerIndex].Cells, rect, startIndex, endIndex);
	}
	public static void ClampTextCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
		if (CurrentTextLayerIndex < 0 || CurrentTextLayerIndex >= TextLayers.Length) return;
		if (endIndex < 0) endIndex = GetTextUsedCellCount(layerIndex);
		Util.ClampCells(TextLayers[layerIndex].Cells, rect, startIndex, endIndex);
	}
	public static void ClampCells (Cell[] cells, IRect rect) => Util.ClampCells(cells, rect, 0, cells.Length);


	// Exclude
	public static void ExcludeCellsForAllLayers (IRect rect) {
		int count = LayerCount;
		for (int i = 0; i < count; i++) {
			ExcludeCellsLogic(Layers[i].Cells, i, rect, 0, GetUsedCellCount(i));
		}
	}
	public static void ExcludeCells (IRect rect, int startIndex, int endIndex = -1) {
		if (endIndex < 0) endIndex = GetUsedCellCount(CurrentLayerIndex);
		ExcludeCellsLogic(Layers[CurrentLayerIndex].Cells, CurrentLayerIndex, rect, startIndex, endIndex);
	}
	public static void ExcludeCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
		if (endIndex < 0) endIndex = GetUsedCellCount(layerIndex);
		ExcludeCellsLogic(Layers[layerIndex].Cells, layerIndex, rect, startIndex, endIndex);
	}
	public static void ExcludeTextCellsForAllLayers (IRect rect) {
		for (int i = 0; i < TextLayers.Length; i++) {
			ExcludeCellsLogic(TextLayers[i].Cells, i, rect, 0, GetTextUsedCellCount(i));
		}
	}
	public static void ExcludeTextCells (IRect rect, int startIndex, int endIndex = -1) {
		if (CurrentTextLayerIndex < 0 || CurrentTextLayerIndex >= TextLayers.Length) return;
		if (endIndex < 0) endIndex = GetTextUsedCellCount(CurrentTextLayerIndex);
		ExcludeCellsLogic(TextLayers[CurrentTextLayerIndex].Cells, CurrentTextLayerIndex, rect, startIndex, endIndex);
	}
	public static void ExcludeTextCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
		if (layerIndex < 0 || layerIndex >= TextLayers.Length) return;
		if (endIndex < 0) endIndex = GetTextUsedCellCount(layerIndex);
		ExcludeCellsLogic(TextLayers[layerIndex].Cells, layerIndex, rect, startIndex, endIndex);
	}
	private static void ExcludeCellsLogic (Cell[] cells, int layerIndex, IRect rect, int startIndex, int endIndex) {

		var cellRect = new IRect();

		for (int i = startIndex; i < endIndex; i++) {

			var cell = cells[i];
			cellRect.x = cell.X - (int)(cell.Width * cell.PivotX);
			cellRect.y = cell.Y - (int)(cell.Height * cell.PivotY);
			cellRect.width = cell.Width;
			cellRect.height = cell.Height;
			cellRect.FlipNegative();

			if (!cellRect.Overlaps(rect)) continue;

			// Complete Inside
			if (
				cellRect.x > rect.x && cellRect.xMax < rect.xMax &&
				cellRect.y > rect.y && cellRect.yMax < rect.yMax
			) {
				cell.Color.a = 0;
				continue;
			}

			// L
			var newCellRect = cellRect;
			if (cellRect.x < rect.x) {
				MakeCell(
					cell, cellRect, cellRect.EdgeInside(Direction4.Left, rect.x - cellRect.x),
					layerIndex
				);
				newCellRect = newCellRect.EdgeInside(Direction4.Right, cellRect.xMax - rect.x);
			}

			// R
			if (cellRect.xMax > rect.xMax) {
				MakeCell(
					cell, cellRect, cellRect.EdgeInside(Direction4.Right, cellRect.xMax - rect.xMax),
					layerIndex
				);
				newCellRect = newCellRect.EdgeInside(Direction4.Left, rect.xMax - cellRect.x);
			}

			// D
			if (cellRect.y < rect.y) {
				MakeCell(
					cell, cellRect, newCellRect.EdgeInside(Direction4.Down, rect.y - cellRect.y),
					layerIndex
				);
			}

			// U
			if (cellRect.yMax > rect.yMax) {
				MakeCell(
					cell, cellRect, newCellRect.EdgeInside(Direction4.Up, cellRect.yMax - rect.yMax),
					layerIndex
				);
			}

			cell.Color.a = 0;

			// Func
			static void MakeCell (Cell source, IRect originalRect, IRect targetRect, int layerIndex) {
				Cell target;
				if (source.Sprite != null) {
					int oldLayer = CurrentLayerIndex;
					SetLayer(layerIndex);
					target = Draw(Const.PIXEL, default, 0);
					SetLayer(oldLayer);
				} else {
					int oldLayer = CurrentTextLayerIndex;
					SetTextLayer(layerIndex);
					target = DrawChar('a', 0, 0, 10, 10, Color32.WHITE);
					SetTextLayer(oldLayer);
				}
				if (target.Sprite == null && target.TextSprite == null) return;
				target.CopyFrom(source);
				target.X = originalRect.x;
				target.Y = originalRect.y;
				target.Width = originalRect.width;
				target.Height = originalRect.height;
				target.PivotX = 0;
				target.PivotY = 0;
				target.Shift.left = Util.Max(source.Shift.left, targetRect.x - originalRect.x);
				target.Shift.right = Util.Max(source.Shift.right, originalRect.xMax - targetRect.xMax);
				target.Shift.down = Util.Max(source.Shift.down, targetRect.y - originalRect.y);
				target.Shift.up = Util.Max(source.Shift.up, originalRect.yMax - targetRect.yMax);
			}

		}
	}


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
	public static bool GetTextCells (out Cell[] cells, out int count) => GetTextCells(CurrentTextLayerIndex, out cells, out count);
	public static bool GetTextCells (int layer, out Cell[] cells, out int count) {
		if (layer >= 0 && layer < TextLayers.Length) {
			var item = TextLayers[layer];
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
	internal static bool RequireCharForPool (char c, out CharSprite charSprite) {
		var tLayer = TextLayers[CurrentTextLayerIndex];
		if (tLayer.TextIDMap.TryGetValue(c, out var textSprite)) {
			// Get Exists
			charSprite = textSprite;
		} else {
			// Require Char from Font
			charSprite = Game.GetCharSprite(CurrentTextLayerIndex, c);
			tLayer.TextIDMap.Add(c, charSprite);
		}
		return charSprite != null;
	}


	#endregion




}
