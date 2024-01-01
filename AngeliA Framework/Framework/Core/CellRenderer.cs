using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	public class Cell {
		public int Index;
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
			Index = other.Index;
			Order = other.Order;
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



	public static partial class CellRenderer {




		#region --- SUB ---


		private class CellComparer : IComparer<Cell> {
			public static readonly CellComparer Instance = new();
			public int Compare (Cell a, Cell b) {
				if (a.Z < b.Z) return -1;
				if (a.Z > b.Z) return 1;
				if (a.Order < b.Order) return -1;
				if (a.Order > b.Order) return 1;
				return 0;
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


		private class TextLayer : Layer {
			public int TextSize = 30;
			public int TextRebuild = 1;
			public readonly Dictionary<int, CellInfo> TextIDMap = new();
			public readonly List<CharSprite> CharSprites = new();
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
					Util.QuickSort(Cells, SortedIndex, Count - 1, CellComparer.Instance);
					SortedIndex = Count;
				}
			}
			public void AbandonZSort () => SortedIndex = Count;
		}


		private class CellInfo {
			public int Index {
				get => GetIndex(Game.GlobalFrame);
				set => _Index = value;
			}
			public int Length => Chain != null ? Chain.Count : 1;
			public int LoopStart => Chain != null ? Chain.LoopStart : -1;
			private int _Index = -1;
			public AngeSpriteChain Chain = null;
			public CellInfo (int index, AngeSpriteChain chain = null) {
				_Index = index;
				Chain = chain;
			}
			public int GetIndex (int frame) => Chain != null ? Chain[GetAnimationFrame(frame, Chain.Count, LoopStart).Clamp(0, Chain.Count - 1)] : _Index;
		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly Cell EMPTY_CELL = new() { Index = -1 };
		private static readonly Byte4 WHITE = new(255, 255, 255, 255);
		private static readonly int[] RENDER_CAPACITY = new int[RenderLayer.COUNT] {
			256,	// Wallpaper 
			8192,	// Behind 
			4096,	// Shadow 
			8192,	// Default 
			256,	// Color 
			128,	// Mult 
			128,	// Add 
			4096,	// UI 
			256,	// TopUI 
		};
		private static readonly string[] LAYER_NAMES = new string[RenderLayer.COUNT] {
			"Wallpaper", "Behind", "Shadow", "Default", "Color", "Mult", "Add", "UI", "TopUI",
		};
		private static readonly bool[] DEFAULT_PART_IGNORE = new bool[9] { false, false, false, false, false, false, false, false, false, };

		// Api
		public static IRect ViewRect { get; private set; } = default;
		public static IRect CameraRect { get; private set; } = default;
		public static float CameraRestrictionRate { get; private set; } = 1f;
		public static int LastDrawnID { get; private set; } = 0;
		public static int LayerCount => Layers.Length;
		public static int SpriteCount => Sprites.Length;
		public static int ChainCount => Chains.Length;
		public static int TextLayerCount => TextLayers.Length;
		public static int CurrentLayerIndex { get; private set; } = 0;
		public static int CurrentTextLayerIndex { get; private set; } = 0;
		public static bool TextReady => TextLayers.Length > 0;

		// Data
		private static readonly Dictionary<int, CellInfo> SheetIDMap = new();
		private static readonly Dictionary<int, int[]> SpriteGroupMap = new();
		private static readonly Dictionary<int, SpriteMeta> MetaPool = new();
		private static readonly Cell[] Last9SlicedCells = new Cell[9];
		private static readonly Layer[] Layers = new Layer[RenderLayer.COUNT];
		private static TextLayer[] TextLayers = new TextLayer[0];
		private static AngeSprite[] Sprites = null;
		private static AngeSpriteChain[] Chains = null;
		private static string[] SheetNames = new string[0];
		private static bool IsDrawing = false;


		#endregion




		#region --- MSG ---


		// Init
		[OnGameInitialize(int.MinValue)]
		internal static void Initialize () {
			InitializePool();
			InitializeLayers();
		}


		private static void InitializeLayers () {

			// Create Layers
			for (int i = 0; i < RenderLayer.COUNT; i++) {
				int capacity = RENDER_CAPACITY[i.Clamp(0, RENDER_CAPACITY.Length - 1)];
				string name = LAYER_NAMES[i];
				int order = i == RenderLayer.TOP_UI ? 2048 : i;
				bool uiLayer = i == RenderLayer.UI || i == RenderLayer.TOP_UI;
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

			// Func
			static Layer CreateLayer (string name, bool uiLayer, int sortingOrder, int renderCapacity, bool textLayer) {
				var cells = new Cell[renderCapacity];
				for (int i = 0; i < renderCapacity; i++) {
					cells[i] = new Cell() {
						Index = -1,
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
					Game.OnLayerUpdate(i, false, layer.Cells, layer.Count, ref prevCellCount);
					layer.PrevCellCount = prevCellCount;
				}
				for (int i = 0; i < TextLayers.Length; i++) {
					var layer = TextLayers[i];
					layer.ZSort();
					int prevCellCount = layer.PrevCellCount;
					Game.OnLayerUpdate(i, true, layer.Cells, layer.Count, ref prevCellCount);
					layer.PrevCellCount = prevCellCount;
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
		}


		#endregion




		#region --- API ---


		public static void InitializePool () {

			var sheet = JsonUtil.LoadOrCreateJson<SpriteSheet>(AngePath.SheetRoot);
			if (sheet == null) return;

			SheetIDMap.Clear();
			MetaPool.Clear();
			SpriteGroupMap.Clear();
			Sprites = sheet.Sprites;
			Chains = sheet.SpriteChains;
			SheetNames = sheet.SheetNames;

			// Add Sprites
			for (int i = 0; i < sheet.Sprites.Length; i++) {
				var sp = sheet.Sprites[i];
				SheetIDMap.TryAdd(sp.GlobalID, new CellInfo(i));
				if (sp.MetaIndex >= 0) {
					MetaPool.TryAdd(sp.GlobalID, sheet.Metas[sp.MetaIndex]);
				}
			}

			// Add Sprite Groups
			for (int i = 0; i < sheet.Groups.Length; i++) {
				var group = sheet.Groups[i];
				if (group != null && group.SpriteIDs != null && group.SpriteIDs.Length > 0) {
					SpriteGroupMap.TryAdd(group.ID, group.SpriteIDs);
				}
			}

			// Add Animated Sprite Chains
			for (int i = 0; i < sheet.SpriteChains.Length; i++) {
				var chain = sheet.SpriteChains[i];
				if (chain.Type != GroupType.Animated || chain.Count == 0) continue;
				SheetIDMap.TryAdd(chain.ID, new(0, chain));
			}

			// Add Meta for Chains
			for (int i = 0; i < sheet.SpriteChains.Length; i++) {
				var chain = sheet.SpriteChains[i];
				int id = chain.ID;
				if (!SheetIDMap.ContainsKey(id)) continue;
				if (chain.Count > 0) {
					int index = chain[0];
					if (index >= 0 && index < sheet.Sprites.Length) {
						var sp = sheet.Sprites[index];
						if (sp.MetaIndex >= 0) {
							MetaPool.TryAdd(id, sheet.Metas[sp.MetaIndex]);
						}
					}
				}
			}
		}


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
		public static void SetLayerToTopUI () => CurrentLayerIndex = RenderLayer.TOP_UI;


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


		public static Cell Draw (int globalID, IRect rect, int z = int.MinValue) => Draw(globalID, false, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, WHITE, z);
		public static Cell Draw (int globalID, IRect rect, Byte4 color, int z = int.MinValue) => Draw(globalID, false, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID, false, x, y, pivotX, pivotY, rotation, width, height, WHITE, z);
		public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) => Draw(globalID, false, x, y, pivotX, pivotY, rotation, width, height, color, z);
		public static Cell Draw (int globalID, bool forText, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) {

			if (!IsDrawing) return EMPTY_CELL;

			CellInfo rCell;
			if (forText) {
				var tLayer = TextLayers[CurrentTextLayerIndex];
				if (!tLayer.TextIDMap.TryGetValue(globalID, out rCell)) {
					return EMPTY_CELL;
				}
			} else {
				if (!SheetIDMap.TryGetValue(globalID, out rCell)) {
					return EMPTY_CELL;
				}
			}
			var layer = forText ?
				TextLayers[CurrentTextLayerIndex.Clamp(0, TextLayers.Length - 1)] :
				Layers[CurrentLayerIndex.Clamp(0, Layers.Length - 1)];
			if (Game.IsPausing && !layer.UiLayer) return EMPTY_CELL;
			if (layer.FocusedCell < 0) return EMPTY_CELL;
			var cell = layer.Cells[layer.FocusedCell];

			if (!forText) {

				var sprite = Sprites[rCell.GetIndex(Game.GlobalFrame)];

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
				cell.Z = sprite.SortingZ;

			} else {
				// For Text
				cell.Z = 0;
			}

			if (z != int.MinValue) cell.Z = z;

			cell.Index = rCell.Index;
			cell.Order = layer.FocusedCell;
			cell.X = x;
			cell.Y = y;
			cell.Width = width;
			cell.Height = height;
			cell.Rotation = rotation;
			cell.PivotX = pivotX / 1000f;
			cell.PivotY = pivotY / 1000f;
			cell.Color = color;
			cell.BorderSide = Alignment.Full;
			cell.Shift = Int4.zero;

			// Final
			layer.FocusedCell++;
			if (layer.FocusedCell >= layer.CellCount) {
				layer.FocusedCell = -1;
			}
			LastDrawnID = globalID;
			return cell;
		}


		public static Cell[] Draw_9Slice (int globalID, IRect rect) => Draw_9Slice(globalID, rect, WHITE);
		public static Cell[] Draw_9Slice (int globalID, IRect rect, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell[] Draw_9Slice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, IRect rect, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Byte4 color, int z = int.MinValue) {
			var border = TryGetSprite(globalID, out var sprite) ? sprite.GlobalBorder : default;
			return Draw_9Slice(
				globalID, x, y, pivotX, pivotY, rotation, width, height,
				border.left, border.right,
				border.down, border.up,
				color, z
			);
		}
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Byte4 color, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, DEFAULT_PART_IGNORE, color, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, bool[] partIgnore, Byte4 color, int z = int.MinValue) {

			Last9SlicedCells[0] = Last9SlicedCells[1] = Last9SlicedCells[2] = EMPTY_CELL;
			Last9SlicedCells[3] = Last9SlicedCells[4] = Last9SlicedCells[5] = EMPTY_CELL;
			Last9SlicedCells[6] = Last9SlicedCells[7] = Last9SlicedCells[8] = EMPTY_CELL;

			// Original Size
			if (width != 0 && height != 0 && TryGetSprite(globalID, out var sprite)) {
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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
						globalID, x, y, 0, 0,
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


		public static Cell DrawAnimation (int chainID, IRect globalRect, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, IRect globalRect, int frame, Byte4 color, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, color, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, Byte4 color, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, color, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, Byte4 color, int loopStart = int.MinValue) {
			if (!SheetIDMap.TryGetValue(chainID, out var rCell)) return EMPTY_CELL;
			int localFrame = GetAnimationFrame(frame, rCell.Length, loopStart == int.MinValue ? rCell.LoopStart : loopStart);
			var sprite = Sprites[rCell.GetIndex(localFrame)];
			return Draw(sprite.GlobalID, x, y, pivotX, pivotY, rotation, width, height, color);
		}


		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int loopStart = -1) => DrawAnimation_9Slice(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, WHITE, loopStart);
		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, Byte4 color, int loopStart = -1) {
			if (!SheetIDMap.TryGetValue(chainID, out var rCell)) return Last9SlicedCells;
			var sprite = Sprites[rCell.GetIndex(GetAnimationFrame(frame, rCell.Length, loopStart))];
			return Draw_9Slice(sprite.GlobalID, x, y, pivotX, pivotY, rotation, width, height, sprite.GlobalBorder.left, sprite.GlobalBorder.right, sprite.GlobalBorder.down, sprite.GlobalBorder.up, color);
		}
		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int borderL, int borderR, int borderD, int borderU, int loopStart = -1) => DrawAnimation_9Slice(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, borderL, borderR, borderD, borderU, WHITE, loopStart);
		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int borderL, int borderR, int borderD, int borderU, Byte4 color, int loopStart = -1) {
			if (!SheetIDMap.TryGetValue(chainID, out var rCell)) return Last9SlicedCells;
			var sprite = Sprites[rCell.GetIndex(GetAnimationFrame(frame, rCell.Length, loopStart))];
			return Draw_9Slice(sprite.GlobalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, color);
		}


		public static void DrawBlackCurtain (int amount) {
			int oldLayer = CurrentLayerIndex;
			SetLayerToTopUI();
			Draw(
				Const.PIXEL,
				CameraRect.Expand(Const.HALF),
				new Byte4(0, 0, 0, (byte)Util.RemapUnclamped(0, 1000, 0, 255, amount).Clamp(0, 255)),
				int.MaxValue
			);
			SetLayer(oldLayer);
		}


		// Sprite Data
		public static bool TryGetSprite (int globalID, out AngeSprite sprite) {
			if (SheetIDMap.TryGetValue(globalID, out var rCell)) {
				sprite = Sprites[rCell.GetIndex(Game.GlobalFrame)];
				return true;
			} else {
				sprite = null;
				return false;
			}
		}


		public static bool HasSpriteGroup (int groupID) => HasSpriteGroup(groupID, out _);
		public static bool HasSpriteGroup (int groupID, out int groupLength) {
			if (SpriteGroupMap.TryGetValue(groupID, out var values)) {
				groupLength = values.Length;
				return true;
			} else {
				groupLength = 0;
				return false;
			}
		}


		public static bool TryGetSpriteFromGroup (int groupID, int index, out AngeSprite sprite, bool loopIndex = true, bool clampIndex = true) {
			int[] ids = null;
			if (ids == null && SpriteGroupMap.TryGetValue(groupID, out ids)) {
				if (loopIndex) index = index.UMod(ids.Length);
				if (clampIndex) index = index.Clamp(0, ids.Length - 1);
				sprite = null;
				return index >= 0 && index < ids.Length && TryGetSprite(ids[index], out sprite);
			} else return TryGetSprite(groupID, out sprite);
		}


		public static bool TryGetMeta (int globalID, out SpriteMeta meta) => MetaPool.TryGetValue(globalID, out meta);


		public static bool HasSprite (int globalID) => SheetIDMap.ContainsKey(globalID);


		public static AngeSprite GetSpriteAt (int index) => index >= 0 && index < Sprites.Length ? Sprites[index] : null;


		public static AngeSpriteChain GetChainAt (int index) => Chains[index];


		public static CharSprite GetCharSprite (int layerIndex, int spriteIndex) => TextLayers[layerIndex].CharSprites[spriteIndex];


		public static int GetCharSpriteCount (int layerIndex) => TextLayers[layerIndex].CharSprites.Count;


		// Misc
		public static string GetSheetName (AngeSprite sprite) => sprite != null && sprite.SheetNameIndex >= 0 && sprite.SheetNameIndex < SheetNames.Length ? SheetNames[sprite.SheetNameIndex] : string.Empty;


		public static void AddTextRebuild (int layerIndex) => TextLayers[layerIndex].TextRebuild++;


		// Clamp
		public static void ClampTextCells (IRect rect, int startIndex, int endIndex) => ClampCellsLogic(TextLayers[CurrentTextLayerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampTextCells (int layerIndex, IRect rect, int startIndex, int endIndex) => ClampCellsLogic(TextLayers[layerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampCells (IRect rect, int startIndex, int endIndex) => ClampCellsLogic(Layers[CurrentLayerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampCells (int layerIndex, IRect rect, int startIndex, int endIndex) => ClampCellsLogic(Layers[layerIndex].Cells, rect, startIndex, endIndex);
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
			if (tLayer.TextIDMap.TryGetValue(c, out var cellInfo)) {
				if (cellInfo.Index < 0) {
					// No CharInfo for this Char
					return false;
				} else {
					charSprite = tLayer.CharSprites[cellInfo.Index];
					if (charSprite.Rebuild != tLayer.TextRebuild) {
						// Need Cache Again
						charSprite.Rebuild = tLayer.TextRebuild;
						Game.FillCharSprite(CurrentTextLayerIndex, c, tLayer.TextSize, charSprite, out bool filled);
						if (!filled) {
							cellInfo.Index = -1;
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
					tLayer.TextIDMap.Add(c, new CellInfo(tLayer.CharSprites.Count));
					tLayer.CharSprites.Add(charSprite);
				} else {
					// No Info
					tLayer.TextIDMap.Add(c, new CellInfo(-1));
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
