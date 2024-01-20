using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	public class Cell {
		public AngeSprite Sprite;
		public CharSprite TextSprite;
		public int Order;
		public int X;
		public int Y;
		public int Z;
		public int Width;
		public int Height;
		public int Rotation;
		public float PivotX;
		public float PivotY;
		public Byte4 Color;
		public Alignment BorderSide;
		public Int4 Shift;
		public void CopyFrom (Cell other) {
			Sprite = other.Sprite;
			TextSprite = other.TextSprite;
			X = other.X;
			Y = other.Y;
			Z = other.Z;
			Width = other.Width;
			Height = other.Height;
			Rotation = other.Rotation;
			PivotX = other.PivotX;
			PivotY = other.PivotY;
			Color = other.Color;
			BorderSide = other.BorderSide;
			Shift = other.Shift;
		}
		public Int2 LocalToGlobal (int localX, int localY) {
			int pOffsetX = (int)(PivotX * Width);
			int pOffsetY = (int)(PivotY * Height);
			if (Rotation == 0) {
				return new Int2(X + localX - pOffsetX, Y + localY - pOffsetY);
			}
			var globalCellV = new Float2(localX, localY).Rotate(Rotation);
			var globalPivotV = new Float2(pOffsetX, pOffsetY).Rotate(Rotation);
			var result = new Float2(X + globalCellV.x - globalPivotV.x, Y + globalCellV.y - globalPivotV.y);
			return result.RoundToInt();
		}
		public Int2 GlobalToLocal (int globalX, int globalY) {
			int pOffsetX = (int)(PivotX * Width);
			int pOffsetY = (int)(PivotY * Height);
			if (Rotation == 0) {
				return new Int2(globalX + pOffsetX - X, globalY + pOffsetY - Y);
			}
			var globalPoint = new Float2(pOffsetX, pOffsetY).Rotate(Rotation);
			var globalOffset = new Float2(globalX - X + globalPoint.x, globalY - Y + globalPoint.y);
			var result = globalOffset.Rotate(-Rotation);
			return result.RoundToInt();
		}
		public void ReturnPivots () {
			if (Rotation == 0) {
				X -= (Width * PivotX).RoundToInt();
				Y -= (Height * PivotY).RoundToInt();
			} else {
				var point = LocalToGlobal(0, 0);
				X = point.x;
				Y = point.y;
			}
			PivotX = 0;
			PivotY = 0;
		}
		public void ReturnPivots (float newPivotX, float newPivotY) {
			if (Rotation == 0) {
				X -= (Width * (PivotX - newPivotX)).RoundToInt();
				Y -= (Height * (PivotY - newPivotY)).RoundToInt();
			} else {
				var point = LocalToGlobal((int)(newPivotX * Width), (int)(newPivotY * Height));
				X = point.x;
				Y = point.y;
			}
			PivotX = newPivotX;
			PivotY = newPivotY;
		}
		public void ReturnPosition (int globalX, int globalY) {
			var localPoint = GlobalToLocal(globalX, globalY);
			PivotX = (float)localPoint.x / Width;
			PivotY = (float)localPoint.y / Height;
			X = globalX;
			Y = globalY;
		}
		public void RotateAround (int rotation, int pointX, int pointY) {
			if (rotation == 0 || Width == 0 || Height == 0) return;
			var localPoint = GlobalToLocal(pointX, pointY);
			PivotX = (float)localPoint.x / Width;
			PivotY = (float)localPoint.y / Height;
			X = pointX;
			Y = pointY;
			Rotation += rotation;
		}
		public void ScaleFrom (int scale, int pointX, int pointY) {
			var localPoint = GlobalToLocal(pointX, pointY);
			PivotX = (float)localPoint.x / Width;
			PivotY = (float)localPoint.y / Height;
			X = pointX;
			Y = pointY;
			Width = Width * scale / 1000;
			Height = Height * scale / 1000;
		}
	}



	public class CharSprite {
		public int GlobalID;
		public Float2 UvBottomLeft;
		public Float2 UvBottomRight;
		public Float2 UvTopLeft;
		public Float2 UvTopRight;
		public FRect Offset;
		public float Advance;
		public int Rebuild = 0;
	}


	public static partial class CellRenderer {




		#region --- SUB ---


		private class CellComparer : IComparer<Cell> {
			public static readonly CellComparer Instance = new();
			public int Compare (Cell a, Cell b) =>
				a.Z < b.Z ? -1 :
				a.Z > b.Z ? 1 :
				a.Order < b.Order ? -1 :
				a.Order > b.Order ? 1 :
				0;
		}


		private class ReversedCellComparer : IComparer<Cell> {
			public static readonly ReversedCellComparer Instance = new();
			public int Compare (Cell a, Cell b) =>
				a.Z < b.Z ? 1 :
				a.Z > b.Z ? -1 :
				a.Order < b.Order ? 1 :
				a.Order > b.Order ? -1 :
				0;
		}


		private class TextLayer : Layer {
			public int TextSize = 30;
			public int TextRebuild = 1;
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
			public void ZSort (bool fromStart = false) {
				if (fromStart) SortedIndex = 0;
				if (SortedIndex < Count - 1) {
					Util.QuickSort(
						Cells, SortedIndex, Count - 1,
						UiLayer ? ReversedCellComparer.Instance : CellComparer.Instance
					);
					SortedIndex = Count;
				}
			}
			public void AbandonZSort () => SortedIndex = Count;
		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly Cell EMPTY_CELL = new() { Sprite = null, TextSprite = null, };
		private static readonly bool[] DEFAULT_PART_IGNORE = new bool[9] { false, false, false, false, false, false, false, false, false, };

		// Api
		public static IRect ViewRect { get; private set; } = default;
		public static IRect CameraRect { get; private set; } = default;
		public static float CameraRestrictionRate { get; private set; } = 1f;
		public static int LastDrawnID { get; private set; } = 0;
		public static int LayerCount => Layers.Length;
		public static int SpriteCount => Sheet.Sprites.Length;
		public static int GroupCount => Sheet.Groups.Length;
		public static int TextLayerCount => TextLayers.Length;
		public static int CurrentLayerIndex { get; private set; } = 0;
		public static int CurrentTextLayerIndex { get; private set; } = 0;
		public static bool TextReady => TextLayers.Length > 0;

		// Data
		private static readonly Sheet Sheet = new();
		private static readonly Cell[] Last9SlicedCells = new Cell[9];
		private static readonly Layer[] Layers = new Layer[RenderLayer.COUNT];
		private static TextLayer[] TextLayers = new TextLayer[0];
		private static bool IsDrawing = false;


		#endregion




		#region --- MSG ---


		// Init
		[OnGameInitialize(-4096)]
		internal static void Initialize () {

			// Create Layers
			for (int i = 0; i < RenderLayer.COUNT; i++) {
				int capacity = RenderLayer.CAPACITY[i];
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
				var tLayer = TextLayers[i] = CreateLayer(
					name,
					uiLayer: true,
					sortingOrder,
					TEXT_CAPACITY,
					textLayer: true
				) as TextLayer;
				tLayer.TextSize = Game.GetFontSize(i).Clamp(42, int.MaxValue);
				Game.OnTextLayerCreated(i, name, sortingOrder, TEXT_CAPACITY);
			}

			// Load Sheet
			Sheet.LoadFromDisk(Project.CurrentProject.SheetPath);
			Game.SetTextureForRenderer(Sheet.Texture);

			// Func
			static Layer CreateLayer (string name, bool uiLayer, int sortingOrder, int renderCapacity, bool textLayer) {
				var cells = new Cell[renderCapacity];
				for (int i = 0; i < renderCapacity; i++) {
					cells[i] = new Cell() {
						Sprite = null,
						BorderSide = Alignment.Full,
					};
				}
				var layer = textLayer ? new TextLayer() : new Layer();
				layer.Name = name;
				layer.Cells = cells;
				layer.CellCount = renderCapacity;
				layer.FocusedCell = 0;
				layer.PrevCellCount = 0;
				layer.SortedIndex = 0;
				layer.SortingOrder = sortingOrder;
				layer.UiLayer = uiLayer;
				return layer;
			}
		}


		// Update
		[OnProjectOpen]
		internal static void OnProjectOpen () {
			if (Game.GlobalFrame == 0) return;
			Sheet.LoadFromDisk(Project.CurrentProject.SheetPath);
			Game.SetTextureForRenderer(Sheet.Texture);
		}


		[OnGameUpdate(-2048)]
		internal static void CameraUpdate () {

			var viewRect = Stage.ViewRect;

			// Ratio
			float ratio = (float)Game.ScreenWidth / Game.ScreenHeight;
			float maxRatio = (float)viewRect.width / viewRect.height;
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
			var cRect = new IRect(
				viewRect.x,
				viewRect.y,
				(int)(viewRect.height * Game.CameraAspect),
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
			try {
				for (int i = 0; i < Layers.Length; i++) {
					var layer = Layers[i];
					layer.ZSort();
					int prevCellCount = layer.PrevCellCount;
					Game.OnLayerUpdate(i, layer.UiLayer, false, layer.Cells, layer.Count, ref prevCellCount);
					layer.PrevCellCount = prevCellCount;
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
			try {
				for (int i = 0; i < TextLayers.Length; i++) {
					var layer = TextLayers[i];
					layer.ZSort();
					int prevCellCount = layer.PrevCellCount;
					Game.OnLayerUpdate(i, layer.UiLayer, true, layer.Cells, layer.Count, ref prevCellCount);
					layer.PrevCellCount = prevCellCount;
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
		}


		#endregion




		#region --- API ---


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
		public static void SetLayerToUI () => CurrentLayerIndex = RenderLayer.UI;


		public static void SortLayer (int layerIndex) => Layers[layerIndex].ZSort();


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
		[OnGameUpdate(-512)]
		public static void BeginDraw () {
			IsDrawing = true;
			EMPTY_CELL.Sprite = null;
			EMPTY_CELL.TextSprite = null;
			SetLayerToDefault();
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


		public static Cell Draw (int globalID, IRect rect, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Const.WHITE, z);
		public static Cell Draw (int globalID, IRect rect, Byte4 color, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID, x, y, pivotX, pivotY, rotation, width, height, Const.WHITE, z);
		public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) => TryGetSprite(globalID, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : EMPTY_CELL;
		public static Cell Draw (SpriteCode globalID, IRect rect, int z = int.MinValue) => Draw(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Const.WHITE, z);
		public static Cell Draw (SpriteCode globalID, IRect rect, Byte4 color, int z = int.MinValue) => Draw(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell Draw (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Const.WHITE, z);
		public static Cell Draw (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) => TryGetSprite(globalID.ID, out var sprite) ? Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color, z) : EMPTY_CELL;
		public static Cell Draw (AngeSprite sprite, IRect rect, int z = int.MinValue) => Draw(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, Const.WHITE, z);
		public static Cell Draw (AngeSprite sprite, IRect rect, Byte4 color, int z = int.MinValue) => Draw(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell Draw (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, Const.WHITE, z);
		public static Cell Draw (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) {

			if (!IsDrawing || sprite == null) return EMPTY_CELL;

			var layer = Layers[CurrentLayerIndex.Clamp(0, Layers.Length - 1)];
			if (Game.IsPausing && !layer.UiLayer) return EMPTY_CELL;
			if (layer.FocusedCell < 0) return EMPTY_CELL;
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
			LastDrawnID = sprite.GlobalID;
			return cell;
		}


		public static Cell DrawChar (char c, int x, int y, int width, int height, Byte4 color) {

			if (!IsDrawing) return EMPTY_CELL;

			var tLayer = TextLayers[CurrentTextLayerIndex];
			if (!tLayer.TextIDMap.TryGetValue(c, out var tSprite)) {
				return EMPTY_CELL;
			}

			var layer = TextLayers[CurrentTextLayerIndex.Clamp(0, TextLayers.Length - 1)];
			if (layer.FocusedCell < 0) return EMPTY_CELL;
			var cell = layer.Cells[layer.FocusedCell];

			cell.Z = 0;
			cell.Sprite = null;
			cell.TextSprite = tSprite;
			cell.Order = layer.FocusedCell;
			cell.X = x;
			cell.Y = y;
			cell.Width = width;
			cell.Height = height;
			cell.Rotation = 0;
			cell.PivotX = 0;
			cell.PivotY = 0;
			cell.Color = color;
			cell.BorderSide = Alignment.Full;
			cell.Shift = Int4.zero;

			// Final
			layer.FocusedCell++;
			if (layer.FocusedCell >= layer.CellCount) {
				layer.FocusedCell = -1;
			}
			LastDrawnID = c;
			return cell;
		}


		public static Cell[] Draw_9Slice (int globalID, IRect rect) => Draw_9Slice(globalID, rect, Const.WHITE);
		public static Cell[] Draw_9Slice (int globalID, IRect rect, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell[] Draw_9Slice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Const.WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, Const.WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) {
			var border = TryGetSprite(globalID, out var sprite) ? sprite.GlobalBorder : default;
			return Draw_9Slice(
				sprite, x, y, pivotX, pivotY, rotation, width, height,
				border.left, border.right,
				border.down, border.up,
				color, z
			);
		}
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Const.WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Byte4 color, int z = int.MinValue) {
			TryGetSprite(globalID, out var sprite);
			return Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, partIgnore, color, z);
		}
		public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect) => Draw_9Slice(globalID.ID, rect, Const.WHITE);
		public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Const.WHITE, z);
		public static Cell[] Draw_9Slice (SpriteCode globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID.ID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
		public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, Const.WHITE, z);
		public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) {
			var border = TryGetSprite(globalID.ID, out var sprite) ? sprite.GlobalBorder : default;
			return Draw_9Slice(
				sprite, x, y, pivotX, pivotY, rotation, width, height,
				border.left, border.right,
				border.down, border.up,
				color, z
			);
		}
		public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Const.WHITE, z);
		public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID.ID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
		public static Cell[] Draw_9Slice (SpriteCode globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Byte4 color, int z = int.MinValue) {
			TryGetSprite(globalID.ID, out var sprite);
			return Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, partIgnore, color, z);
		}
		public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect) => Draw_9Slice(sprite, rect, Const.WHITE);
		public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect, Byte4 color, int z = int.MinValue) => Draw_9Slice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, Const.WHITE, z);
		public static Cell[] Draw_9Slice (AngeSprite sprite, IRect rect, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(sprite, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
		public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, Const.WHITE, z);
		public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) {
			return Draw_9Slice(
				sprite, x, y, pivotX, pivotY, rotation, width, height,
				sprite.GlobalBorder.left, sprite.GlobalBorder.right,
				sprite.GlobalBorder.down, sprite.GlobalBorder.up,
				color, z
			);
		}
		public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, Const.WHITE, z);
		public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(sprite, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
		public static Cell[] Draw_9Slice (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Byte4 color, int z = int.MinValue) {

			Last9SlicedCells[0] = Last9SlicedCells[1] = Last9SlicedCells[2] = EMPTY_CELL;
			Last9SlicedCells[3] = Last9SlicedCells[4] = Last9SlicedCells[5] = EMPTY_CELL;
			Last9SlicedCells[6] = Last9SlicedCells[7] = Last9SlicedCells[8] = EMPTY_CELL;

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
			} else return Last9SlicedCells;

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
						rotation, borderL, borderU, color
					);
					cell.BorderSide = Alignment.TopLeft;
					cell.PivotX = _px0;
					cell.PivotY = _py;
					Last9SlicedCells[0] = cell;
				}
				// TM
				if (hasM && !partIgnore[1]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, mWidth, borderU, color
					);
					cell.BorderSide = Alignment.TopMid;
					cell.PivotX = _px1;
					cell.PivotY = _py;
					Last9SlicedCells[1] = cell;
				}
				// TR
				if (hasR && !partIgnore[2]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, borderR, borderU, color
					);
					cell.PivotX = _px2;
					cell.PivotY = _py;
					cell.BorderSide = Alignment.TopRight;
					Last9SlicedCells[2] = cell;
				}
			}
			if (borderD + borderU < height) {
				float _py = (height * pivotY / 1000f - borderD) / mHeight;
				// ML
				if (hasL && !partIgnore[3]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, borderL, mHeight, color
					);
					cell.BorderSide = Alignment.MidLeft;
					cell.PivotX = _px0;
					cell.PivotY = _py;
					Last9SlicedCells[3] = cell;
				}
				// MM
				if (hasM && !partIgnore[4]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, mWidth, mHeight, color
					);
					cell.BorderSide = Alignment.MidMid;
					cell.PivotX = _px1;
					cell.PivotY = _py;
					Last9SlicedCells[4] = cell;
				}
				// MR
				if (hasR && !partIgnore[5]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, borderR, mHeight, color
					);
					cell.BorderSide = Alignment.MidRight;
					cell.PivotX = _px2;
					cell.PivotY = _py;
					Last9SlicedCells[5] = cell;
				}
			}
			if (borderD > 0) {
				float _py = height * pivotY / 1000f / borderD;
				// DL
				if (hasL && !partIgnore[6]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, borderL, borderD, color
					);
					cell.BorderSide = Alignment.BottomLeft;
					cell.PivotX = _px0;
					cell.PivotY = _py;
					Last9SlicedCells[6] = cell;
				}
				// DM
				if (hasM && !partIgnore[7]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, mWidth, borderD, color
					);
					cell.BorderSide = Alignment.BottomMid;
					cell.PivotX = _px1;
					cell.PivotY = _py;
					Last9SlicedCells[7] = cell;
				}
				// DR
				if (hasR && !partIgnore[8]) {
					var cell = Draw(
						sprite, x, y, 0, 0,
						rotation, borderR, borderD, color
					);
					cell.BorderSide = Alignment.BottomRight;
					cell.PivotX = _px2;
					cell.PivotY = _py;
					Last9SlicedCells[8] = cell;
				}
			}

			// Flip for Negative Size
			if (flipX) {
				foreach (var cell in Last9SlicedCells) cell.Width = -cell.Width;
			}
			if (flipY) {
				foreach (var cell in Last9SlicedCells) cell.Height = -cell.Height;
			}

			// Z
			if (z != int.MinValue) foreach (var cell in Last9SlicedCells) cell.Z = z;

			EMPTY_CELL.Color = Const.CLEAR;
			return Last9SlicedCells;
		}


		public static Cell DrawAnimation (int chainID, IRect globalRect, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, Const.WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, Const.WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, Const.WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, IRect globalRect, int frame, Byte4 color, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, color, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, Byte4 color, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, color, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, Byte4 color, int loopStart = int.MinValue) {
			if (!TryGetSpriteGroup(chainID, out var group) || group.Type != GroupType.Animated) return EMPTY_CELL;
			int localFrame = GetAnimationFrame(frame, group.Length, loopStart == int.MinValue ? group.LoopStart : loopStart);
			var sprite = group[localFrame];
			return Draw(sprite, x, y, pivotX, pivotY, rotation, width, height, color);
		}


		// Sprite Data
		public static bool TryGetSprite (int globalID, out AngeSprite sprite, bool ignoreAnimation = false) => TryGetSprite(Sheet, globalID, out sprite, ignoreAnimation);
		public static bool TryGetSprite (Sheet sheet, int globalID, out AngeSprite sprite, bool ignoreAnimation = false) {
			if (sheet.SpritePool.TryGetValue(globalID, out sprite)) return true;
			if (!ignoreAnimation && sheet.GroupPool.TryGetValue(globalID, out var group) && group.Type == GroupType.Animated) {
				int localFrame = GetAnimationFrame(Game.GlobalFrame, group.Length, group.LoopStart);
				sprite = group[localFrame];
				return true;
			}
			sprite = null;
			return false;
		}


		public static bool HasSpriteGroup (int groupID) => HasSpriteGroup(Sheet, groupID);
		public static bool HasSpriteGroup (Sheet sheet, int groupID) => sheet.GroupPool.ContainsKey(groupID);

		public static bool HasSpriteGroup (int groupID, out int groupLength) => HasSpriteGroup(Sheet, groupID, out groupLength);
		public static bool HasSpriteGroup (Sheet sheet, int groupID, out int groupLength) {
			if (sheet.GroupPool.TryGetValue(groupID, out var values)) {
				groupLength = values.Length;
				return true;
			} else {
				groupLength = 0;
				return false;
			}
		}


		public static bool TryGetSpriteGroup (int groupID, out SpriteGroup group) => TryGetSpriteGroup(Sheet, groupID, out group);
		public static bool TryGetSpriteGroup (Sheet sheet, int groupID, out SpriteGroup group) => sheet.GroupPool.TryGetValue(groupID, out group);


		public static bool TryGetSpriteFromGroup (int groupID, int index, out AngeSprite sprite, bool loopIndex = true, bool clampIndex = true) => TryGetSpriteFromGroup(Sheet, groupID, index, out sprite, loopIndex, clampIndex);
		public static bool TryGetSpriteFromGroup (Sheet sheet, int groupID, int index, out AngeSprite sprite, bool loopIndex = true, bool clampIndex = true) {
			if (sheet.GroupPool.TryGetValue(groupID, out var sprites)) {
				if (loopIndex) index = index.UMod(sprites.Length);
				if (clampIndex) index = index.Clamp(0, sprites.Length - 1);
				if (index >= 0 && index < sprites.Length) {
					sprite = sprites[index];
					return true;
				} else {
					sprite = null;
					return false;
				}
			} else return TryGetSprite(sheet, groupID, out sprite, ignoreAnimation: true);
		}


		public static bool HasSprite (int globalID) => HasSprite(Sheet, globalID);
		public static bool HasSprite (Sheet sheet, int globalID) => sheet.SpritePool.ContainsKey(globalID);


		public static AngeSprite GetSpriteAt (int index) => index >= 0 && index < Sheet.Sprites.Length ? Sheet.Sprites[index] : null;


		public static SpriteGroup GetGroupAt (int index) => index >= 0 && index < Sheet.Groups.Length ? Sheet.Groups[index] : null;


		// Misc
		public static void AddTextRebuild (int layerIndex) => TextLayers[layerIndex].TextRebuild++;


		// Clamp
		public static void ClampCells (IRect rect, int startIndex, int endIndex = -1) {
			if (endIndex < 0) endIndex = GetUsedCellCount(CurrentLayerIndex);
			ClampCellsLogic(Layers[CurrentLayerIndex].Cells, rect, startIndex, endIndex);
		}
		public static void ClampCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
			if (endIndex < 0) endIndex = GetUsedCellCount(layerIndex);
			ClampCellsLogic(Layers[layerIndex].Cells, rect, startIndex, endIndex);
		}
		public static void ClampTextCells (IRect rect, int startIndex, int endIndex = -1) {
			if (endIndex < 0) endIndex = GetTextUsedCellCount(CurrentTextLayerIndex);
			ClampCellsLogic(TextLayers[CurrentTextLayerIndex].Cells, rect, startIndex, endIndex);
		}
		public static void ClampTextCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
			if (endIndex < 0) endIndex = GetTextUsedCellCount(layerIndex);
			ClampCellsLogic(TextLayers[layerIndex].Cells, rect, startIndex, endIndex);
		}
		public static void ClampCells (Cell[] cells, IRect rect) => ClampCellsLogic(cells, rect, 0, cells.Length);
		private static void ClampCellsLogic (Cell[] cells, IRect rect, int startIndex, int endIndex) {
			var cellRect = new IRect();
			for (int i = startIndex; i < endIndex; i++) {
				var cell = cells[i];
				cellRect.x = cell.X - (int)(cell.Width * cell.PivotX);
				cellRect.y = cell.Y - (int)(cell.Height * cell.PivotY);
				cellRect.width = cell.Width;
				cellRect.height = cell.Height;
				cellRect.FlipNegative();
				if (!cellRect.Overlaps(rect)) {
					cell.Width = 0;
					continue;
				}
				// Clamp
				int cellL = cellRect.x;
				int cellR = cellRect.x + cellRect.width;
				int cellD = cellRect.y;
				int cellU = cellRect.y + cellRect.height;
				if (cellL < rect.x) {
					if (cell.Width > 0) {
						cell.Shift.left = rect.x - cellL;
					} else {
						cell.Shift.right = rect.x - cellL;
					}
				}
				if (cellR > rect.x + rect.width) {
					if (cell.Width > 0) {
						cell.Shift.right = cellR - rect.x - rect.width;
					} else {
						cell.Shift.left = cellR - rect.x - rect.width;
					}
				}
				if (cellD < rect.y) {
					if (cell.Height > 0) {
						cell.Shift.down = rect.y - cellD;
					} else {
						cell.Shift.up = rect.y - cellD;
					}
				}
				if (cellU > rect.y + rect.height) {
					if (cell.Height > 0) {
						cell.Shift.up = cellU - rect.y - rect.height;
					} else {
						cell.Shift.down = cellU - rect.y - rect.height;
					}
				}
			}
		}


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
			int count = TextLayerCount;
			for (int i = 0; i < count; i++) {
				ExcludeCellsLogic(TextLayers[i].Cells, i, rect, 0, GetTextUsedCellCount(i));
			}
		}
		public static void ExcludeTextCells (IRect rect, int startIndex, int endIndex = -1) {
			if (endIndex < 0) endIndex = GetTextUsedCellCount(CurrentTextLayerIndex);
			ExcludeCellsLogic(TextLayers[CurrentTextLayerIndex].Cells, CurrentTextLayerIndex, rect, startIndex, endIndex);
		}
		public static void ExcludeTextCells (int layerIndex, IRect rect, int startIndex, int endIndex = -1) {
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
				if (cellRect.Overlaps(rect)) {

					// Inside
					if (
						cellRect.x > rect.x && cellRect.xMax < rect.xMax &&
						cellRect.y > rect.y && cellRect.yMax < rect.yMax
					) {
						cell.Color = Const.CLEAR;
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
					cell.Color = Const.CLEAR;

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
							target = DrawChar(' ', 0, 0, 1, 1, Const.WHITE);
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
			if (layer >= 0 && layer < TextLayerCount) {
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


		#endregion




		#region --- LGC ---


		private static int GetAnimationFrame (int frame, int length, int loopStart = -1) {
			if (frame < 0) frame = frame.UMod(length);
			if (frame >= loopStart && loopStart >= 0 && loopStart < length) {
				frame = (frame - loopStart).UMod(length - loopStart) + loopStart;
			}
			return frame.Clamp(0, length - 1);
		}


		// Text Logic
		internal static bool RequireCharForPool (char c, out CharSprite charSprite) {
			charSprite = null;
			var tLayer = TextLayers[CurrentTextLayerIndex];
			if (tLayer.TextIDMap.TryGetValue(c, out var textSprite)) {
				if (textSprite == null) {
					// No CharInfo for this Char
					return false;
				} else {
					charSprite = textSprite;
					if (charSprite.Rebuild != tLayer.TextRebuild) {
						// Need Cache Again
						charSprite.Rebuild = tLayer.TextRebuild;
						Game.FillCharSprite(
							CurrentTextLayerIndex, c, tLayer.TextSize, charSprite, out bool filled
						);
						if (!filled) {
							tLayer.TextIDMap[c] = null;
						}
					}
					return true;
				}
			} else {
				// Require Char from Font
				charSprite = Game.FillCharSprite(CurrentTextLayerIndex, c, tLayer.TextSize, null, out bool filled);
				if (filled && charSprite != null) {
					// Got Info
					charSprite.Rebuild = tLayer.TextRebuild;
					tLayer.TextIDMap.Add(c, charSprite);
				} else {
					// No Info
					tLayer.TextIDMap.Add(c, null);
				}
				return filled;
			}
		}


		internal static void RequestStringForFont (string content) => Game.RequestStringForFont(
			CurrentTextLayerIndex, TextLayers[CurrentTextLayerIndex].TextSize, content
		);


		internal static void RequestStringForFont (char[] content) => Game.RequestStringForFont(
			CurrentTextLayerIndex, TextLayers[CurrentTextLayerIndex].TextSize, content
		);


		#endregion




	}
}
