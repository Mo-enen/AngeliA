using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public abstract class CellEffect {
		public int Duration { get; set; } = 0;
		protected int LocalFrame { get; private set; } = 0;
		public int SortedIndex { get; set; } = 0;
		public CellEffect (int duration = 0) => Duration = duration;
		public virtual void Start () { LocalFrame = 0; }
		public abstract void Perform (Cell[] cells, int cellCount, int layerIndex);
		public bool Next () {
			LocalFrame++;
			return LocalFrame < Duration;
		}
	}



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
		public Color32 Color;
		public Alignment BorderSide;
		public Vector4Int Shift;
		public Vector2Int LocalToGlobal (int cellX, int cellY) {
			if (Rotation == 0) {
				return new Vector2Int(
					X + cellX - (int)(Width * PivotX),
					Y + cellY - (int)(Height * PivotY)
				);
			}
			int pOffsetX = (int)(PivotX * Width);
			int pOffsetY = (int)(PivotY * Height);
			cellX -= pOffsetX;
			cellY -= pOffsetY;
			Vector2 result = Matrix4x4.TRS(
				new Vector3(X - pOffsetX, Y - pOffsetY, 0),
				Quaternion.Euler(0, 0, -Rotation),
				Vector3.one
			).MultiplyPoint(new Vector3(cellX, cellY, 0f));
			result.x += pOffsetX;
			result.y += pOffsetY;
			return result.RoundToInt();
		}
		public Vector2Int GlobalToLocal (int globalX, int globalY) {
			if (Rotation == 0) {
				return new Vector2Int(
					globalX + (int)(Width * PivotX) - X,
					globalY + (int)(Height * PivotY) - Y
				);
			}
			int pOffsetX = (int)(PivotX * Width);
			int pOffsetY = (int)(PivotY * Height);
			globalX -= pOffsetX;
			globalY -= pOffsetY;
			Vector2 result = Matrix4x4.TRS(
				new Vector3(X - pOffsetX, Y - pOffsetY, 0),
				Quaternion.Euler(0, 0, -Rotation),
				Vector3.one
			).inverse.MultiplyPoint(new Vector3(globalX, globalY, 0f));
			result.x += pOffsetX;
			result.y += pOffsetY;
			return result.RoundToInt();
		}
		public RectInt GetBounds () {
			var p0 = LocalToGlobal(0, 0);
			var p1 = LocalToGlobal(Width, 0);
			var p2 = LocalToGlobal(Width, Height);
			var p3 = LocalToGlobal(0, Height);
			var result = new RectInt();
			result.SetMinMax(
				Mathf.Min(Mathf.Min(p0.x, p1.x), Mathf.Min(p2.x, p3.x)),
				Mathf.Max(Mathf.Max(p0.x, p1.x), Mathf.Max(p2.x, p3.x)),
				Mathf.Min(Mathf.Min(p0.y, p1.y), Mathf.Min(p2.y, p3.y)),
				Mathf.Max(Mathf.Max(p0.y, p1.y), Mathf.Max(p2.y, p3.y))
			);
			return result;
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
		public void RotateAround (int rotation, int pointX, int pointY) {
			if (rotation == Rotation || Width == 0 || Height == 0) return;
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


		internal class CharSprite {
			public int GlobalID;
			public Vector2 UvBottomLeft;
			public Vector2 UvBottomRight;
			public Vector2 UvTopLeft;
			public Vector2 UvTopRight;
			public Rect Offset;
			public float Advance;
			public int Rebuild = 0;
		}


		private class TextLayer : Layer {
			public Font TextFont = null;
			public int TextSize = 30;
			public int TextRebuild = 1;
			public readonly Dictionary<int, CellInfo> TextIDMap = new();
			public readonly List<CharSprite> CharSprites = new();
		}


		private class Layer {
			public int Count => Mathf.Min(Cells.Length, FocusedCell >= 0 ? FocusedCell : Cells.Length);
			public Cell[] Cells;
			public Material Material;
			public int CellCount;
			public int FocusedCell;
			public int PrevCellCount;
			public int SortedIndex;
			public int SortingOrder;
			public bool UiLayer;
			public Transform RendererRoot;
			public MeshRenderer Renderer;
			public Mesh Mesh;
			public List<Vector3> VertexCache;
			public List<Vector2> UvCache;
			public List<Color32> ColorCache;

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


			// Api
			public int Index {
				get => GetIndex(GlobalFrame);
				set => _Index = value;
			}

			public int Length => Chain != null ? Chain.Count : 1;
			public int LoopStart => Chain != null ? Chain.LoopStart : -1;

			// Ser-Api
			private int _Index = -1;
			public AngeSpriteChain Chain = null;

			// API
			public CellInfo (int index, AngeSpriteChain chain = null) {
				_Index = index;
				Chain = chain;
			}


			public int GetIndex (int frame) => Chain != null ? Chain[frame % Chain.Count] : _Index;


		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly Cell EMPTY_CELL = new() { Index = -1 };
		private const int EFFECT_COUNT = 8;
		private const int MAX_CAMERA_SHAKE = 120;
		private static readonly Color32 WHITE = new(255, 255, 255, 255);
		private static readonly int SKYBOX_TOP = Shader.PropertyToID("_ColorA");
		private static readonly int SKYBOX_BOTTOM = Shader.PropertyToID("_ColorB");
		private static readonly Shader SKYBOX_SHADER = Shader.Find("Angelia/Skybox");
		private static readonly Shader[] RENDERING_SHADERS = new Shader[Const.RENDER_LAYER_COUNT] {
			Shader.Find("Angelia/Lerp"),// Wallpaper
			Shader.Find("Angelia/Lerp"),// Behind
			Shader.Find("Angelia/Cell"),// Cell(Default)
			Shader.Find("Angelia/Color"),// Color
			Shader.Find("Angelia/Mult"),// Mult
			Shader.Find("Angelia/Add"), // Add
			Shader.Find("Angelia/Cell"),// UI
			Shader.Find("Angelia/Cell"),// TopUI
		};
		private static readonly int[] RENDER_CAPACITY = new int[Const.RENDER_LAYER_COUNT] { 256, 4096, 4096, 256, 128, 128, 4096, 256, };
		private static readonly string[] LAYER_NAMES = new string[Const.RENDER_LAYER_COUNT] { "Wallpaper", "Behind", "Default", "Color", "Mult", "Add", "UI", "TopUI", };

		// Api
		public static RectInt ViewRect { get; private set; } = default;
		public static RectInt CameraRect { get; private set; } = default;
		public static Color32 SkyTintTop { get; private set; } = default;
		public static Color32 SkyTintBottom { get; private set; } = default;
		public static int LastDrawnID { get; private set; } = 0;
		public static int LayerCount => Layers.Length;
		public static int SpriteCount => Sprites.Length;
		public static int ChainCount => Chains.Length;
		public static int TextLayerCount => TextLayers.Length;
		public static int CurrentLayerIndex { get; private set; } = 0;
		public static int CurrentTextLayerIndex { get; private set; } = 0;
		public static bool CameraShaking => GlobalFrame < CameraShakeEndFrame;
		public static bool TextReady => TextLayers.Length > 0;

		// Data
		private static readonly CellEffect[] Effects = new CellEffect[EFFECT_COUNT];
		private static readonly Dictionary<int, CellInfo> SheetIDMap = new();
		private static readonly Dictionary<int, int[]> SpriteGroupMap = new();
		private static readonly Dictionary<int, SpriteMeta> MetaPool = new();
		private static readonly Cell[] Last9SlicedCells = new Cell[9];
		private static readonly System.Random CameraShakeRandom = new(032481);
		private static AngeSprite[] Sprites = null;
		private static AngeSpriteChain[] Chains = null;
		private static Layer[] Layers = new Layer[0];
		private static TextLayer[] TextLayers = new TextLayer[0];
		private static Material Skybox = null;
		private static int GlobalFrame = 0;
		private static int CameraShakeEndFrame = 0;
		private static int CameraShakeAmount = 1000;
		private static bool IsPausing = false;
		private static bool IsDrawing = false;


		#endregion




		#region --- MSG ---


		// Init
		public static void Initialize_Rendering (Camera camera) {


			SheetIDMap.Clear();
			MetaPool.Clear();
			SpriteGroupMap.Clear();

			var sheet = AngeUtil.LoadOrCreateJson<SpriteSheet>(AngePath.SheetRoot);
			if (sheet == null) return;

			var sheetTexture = AngeUtil.LoadSheetTexture();
			if (sheetTexture == null) return;

			Sprites = sheet.Sprites;
			Chains = sheet.SpriteChains;
			Layers = new Layer[Const.RENDER_LAYER_COUNT];

			// Layers
			for (int i = 0; i < Const.RENDER_LAYER_COUNT; i++) {
				var shader = RENDERING_SHADERS[i];
				int rCapacity = RENDER_CAPACITY[i.Clamp(0, RENDER_CAPACITY.Length - 1)];
				Layers[i] = CreateLayer(
					camera,
					sheet.GetMaterial(shader, sheetTexture),
					LAYER_NAMES[i],
					uiLayer: i == Const.RENDER_LAYER_UI || i == Const.RENDER_LAYER_TOP_UI,
					sortingOrder: i == Const.RENDER_LAYER_TOP_UI ? 2048 : i,
					rCapacity,
					textLayer: false
				);
			}

			if (SKYBOX_SHADER != null) {
				RenderSettings.skybox = Skybox = new Material(SKYBOX_SHADER);
			}

			// Add Sprites
			for (int i = 0; i < sheet.Sprites.Length; i++) {
				var sp = sheet.Sprites[i];
				SheetIDMap.TryAdd(sp.GlobalID, new(i));
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


		public static void Initialize_Text (Camera camera, Font[] fonts) {

			const int capacity = 2048;
			if (fonts == null || fonts.Length == 0) {
				fonts = new Font[1] { Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") };
			}

			if (fonts == null || fonts.Length == 0) return;

			TextLayers = new TextLayer[fonts.Length];
			for (int i = 0; i < fonts.Length; i++) {
				var font = fonts[i];
#pragma warning disable IDE0270
				if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#pragma warning restore IDE0270
				var tLayer = TextLayers[i] = CreateLayer(
					camera, font.material, font.name,
					uiLayer: true,
					sortingOrder: Layers.Length + i,
					capacity,
					textLayer: true
				) as TextLayer;
				if (font != null) {
					tLayer.TextFont = font;
					tLayer.TextSize = font.fontSize.Clamp(42, int.MaxValue);
					Font.textureRebuilt += (_font) => {
						if (_font == tLayer.TextFont) tLayer.TextRebuild++;
					};
				}
			}
		}


		private static Layer CreateLayer (Camera camera, Material material, string name, bool uiLayer, int sortingOrder, int renderCapacity, bool textLayer) {

			if (material == null) {
				material = new Material(Shader.Find("Angelia/Cell"));
			}

			var tf = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)).transform;
			tf.SetParent(camera.transform);
			tf.SetAsLastSibling();
			tf.SetPositionAndRotation(new Vector3(0, 0, 1), Quaternion.identity);
			tf.localScale = Vector3.one;
			var filter = tf.GetComponent<MeshFilter>();
			filter.sharedMesh = new Mesh();
			var mr = tf.GetComponent<MeshRenderer>();
			mr.material = material;
			mr.receiveShadows = false;
			mr.staticShadowCaster = false;
			mr.allowOcclusionWhenDynamic = false;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			mr.sortingOrder = sortingOrder;
			var cells = new Cell[renderCapacity];
			for (int i = 0; i < renderCapacity; i++) {
				cells[i] = new Cell() {
					Index = -1,
					BorderSide = Alignment.Full,
				};
			}

			// Create Layer
			var layer = textLayer ? new TextLayer() : new Layer();
			layer.Cells = cells;
			layer.Mesh = filter.sharedMesh;
			layer.RendererRoot = filter.transform;
			layer.CellCount = renderCapacity;
			layer.FocusedCell = 0;
			layer.VertexCache = new();
			layer.UvCache = new();
			layer.ColorCache = new();
			layer.PrevCellCount = 0;
			layer.Material = material;
			layer.SortedIndex = 0;
			layer.SortingOrder = sortingOrder;
			layer.Renderer = mr;
			layer.UiLayer = uiLayer;

			// Init Mesh
			var tris = new int[renderCapacity * 2 * 3];
			for (int i = 0; i < renderCapacity; i++) {
				tris[i * 6 + 0] = i * 4 + 0;
				tris[i * 6 + 1] = i * 4 + 1;
				tris[i * 6 + 2] = i * 4 + 2;
				tris[i * 6 + 3] = i * 4 + 0;
				tris[i * 6 + 4] = i * 4 + 2;
				tris[i * 6 + 5] = i * 4 + 3;
			}
			layer.VertexCache.AddRange(new Vector3[renderCapacity * 4]);
			layer.UvCache.AddRange(new Vector2[renderCapacity * 4]);
			layer.ColorCache.AddRange(new Color32[renderCapacity * 4]);
			var mesh = filter.sharedMesh;
			mesh.MarkDynamic();
			mesh.SetVertices(layer.VertexCache);
			mesh.SetUVs(0, layer.UvCache);
			mesh.SetColors(layer.ColorCache);
			mesh.SetTriangles(tris, 0);
			mesh.UploadMeshData(false);
			return layer;
		}


		// Update
		internal static void CameraUpdate (Camera camera, RectInt viewRect) {

			ViewRect = viewRect;

			// Ratio
			float ratio = (float)Screen.width / Screen.height;
			float maxRatio = (float)ViewRect.width / ViewRect.height;
			var rect = new Rect(0f, 0f, 1f, 1f);
			if (ratio > maxRatio) {
				rect = new Rect(0.5f - 0.5f * maxRatio / ratio, 0f, maxRatio / ratio, 1f);
			}
			if (camera.rect.NotAlmost(rect)) {
				camera.rect = rect;
			}

			// Camera Rect
			var cRect = new RectInt(
				ViewRect.x,
				ViewRect.y,
				(int)(ViewRect.height * camera.aspect),
				ViewRect.height
			);
			int cOffsetX = (ViewRect.width - cRect.width) / 2;
			cRect.x += cOffsetX;
			CameraRect = cRect;

		}


		internal static void FrameUpdate (int globalFrame, Camera camera) {

			IsDrawing = false;
			GlobalFrame = globalFrame;

			// Effect
			for (int i = 0; i < EFFECT_COUNT; i++) {
				try {
					var eff = Effects[i];
					if (eff == null) continue;
					for (int layerIndex = 0; layerIndex < Layers.Length; layerIndex++) {
						var layer = Layers[layerIndex];
						eff.SortedIndex = layer.SortedIndex;
						eff.Perform(layer.Cells, layer.Count, layerIndex);
					}
					if (!eff.Next()) Effects[i] = null;
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Layer Game Objects
			var pos = Vector3.zero;
			var scl = Vector3.one;
			if (camera != null) {
				pos = new Vector3(
					-camera.orthographicSize * camera.aspect,
					-camera.orthographicSize,
					1f
				);
				pos.x -= ((ViewRect.width - CameraRect.width) / 2) * camera.orthographicSize * 2f * camera.aspect / CameraRect.width;
				scl = new Vector3(
					camera.orthographicSize * 2f / ViewRect.height,
					camera.orthographicSize * 2f / ViewRect.height,
					1f
				);
			}
			for (int layerIndex = 0; layerIndex < Layers.Length; layerIndex++) {
				var layer = Layers[layerIndex];
				layer.RendererRoot.localPosition = pos;
				layer.RendererRoot.localScale = scl;
			}
			for (int layerIndex = 0; layerIndex < TextLayers.Length; layerIndex++) {
				var tLayer = TextLayers[layerIndex];
				tLayer.RendererRoot.localPosition = pos;
				tLayer.RendererRoot.localScale = scl;
			}

			// Camera Shake
			if (GlobalFrame < CameraShakeEndFrame) {
				float amount = CameraShakeAmount * 0.000013f * Util.Remap(
					12, 0, 1f, 0f, CameraShakeEndFrame - GlobalFrame
				);
				for (int layerIndex = 0; layerIndex < Layers.Length - 1; layerIndex++) {
					pos *= Util.Remap(
						-Const.CEL, Const.CEL,
						1f - amount / 2f, 1f + amount / 2f,
						CameraShakeRandom.Next(-Const.CEL, Const.CEL)
					);
					var layer = Layers[layerIndex];
					layer.RendererRoot.localPosition = pos;
					layer.RendererRoot.localScale = scl;
				}
			}

			// Update Mesh
			try {
				for (int i = 0; i < Layers.Length; i++) {
					UpdateLayer(Layers[i]);
				}
				for (int i = 0; i < TextLayers.Length; i++) {
					UpdateLayer(TextLayers[i]);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }

		}


		private static void UpdateLayer (Layer layer) {

			var textLayer = layer as TextLayer;

			// Z-Sort
			layer.ZSort();

			// Mesh
			var mesh = layer.Mesh;
			var cells = layer.Cells;
			int cellCount = layer.Count;

			Vector3 a = Vector3.zero;
			Vector3 b = Vector3.zero;
			Vector3 c = Vector3.zero;
			Vector3 d = Vector3.zero;
			Vector2 uv0;
			Vector2 uv1;
			Vector2 uv2;
			Vector2 uv3;
			int i0, i1, i2, i3;
			float shiftL;
			float shiftR;
			float shiftD;
			float shiftU;

			for (int i = 0; i < cellCount; i++) {

				var cell = cells[i];

				if (cell.Index < 0) continue;
				if (textLayer != null && cell.Index >= textLayer.CharSprites.Count) continue;

				bool shifted = !cell.Shift.IsZero;
				if (shifted) {
					shiftL = ((float)cell.Shift.left / cell.Width.Abs()).Clamp01();
					shiftR = ((float)cell.Shift.right / cell.Width.Abs()).Clamp01();
					shiftD = ((float)cell.Shift.down / cell.Height.Abs()).Clamp01();
					shiftU = ((float)cell.Shift.up / cell.Height.Abs()).Clamp01();
				} else {
					shiftL = 0;
					shiftR = 0;
					shiftD = 0;
					shiftU = 0;
				}

				// Position
				// b c
				// a d
				float pX = cell.Width * cell.PivotX;
				float pY = cell.Height * cell.PivotY;
				if (pX.NotAlmostZero()) {
					a.x = -pX;
					b.x = -pX;
					c.x = cell.Width - pX;
					d.x = cell.Width - pX;
				} else {
					a.x = 0;
					b.x = 0;
					c.x = cell.Width;
					d.x = cell.Width;
				}
				if (pY.NotAlmostZero()) {
					a.y = -pY;
					b.y = cell.Height - pY;
					c.y = cell.Height - pY;
					d.y = -pY;
				} else {
					a.y = 0;
					b.y = cell.Height;
					c.y = cell.Height;
					d.y = 0;
				}

				// Shift Pos
				if (shifted) {
					a.x = b.x = Mathf.Lerp(a.x, d.x, shiftL);
					c.x = d.x = Mathf.Lerp(d.x, a.x, shiftR);
					a.y = d.y = Mathf.Lerp(a.y, b.y, shiftD);
					b.y = c.y = Mathf.Lerp(b.y, a.y, shiftU);
				}

				// Rotation
				if (cell.Rotation != 0) {
					var rot = Quaternion.Euler(0, 0, -cell.Rotation);
					a = rot * a;
					b = rot * b;
					c = rot * c;
					d = rot * d;
				}

				// Global to View
				a.x += cell.X - ViewRect.x;
				a.y += cell.Y - ViewRect.y;
				b.x += cell.X - ViewRect.x;
				b.y += cell.Y - ViewRect.y;
				c.x += cell.X - ViewRect.x;
				c.y += cell.Y - ViewRect.y;
				d.x += cell.X - ViewRect.x;
				d.y += cell.Y - ViewRect.y;

				i0 = i * 4 + 0;
				i1 = i * 4 + 1;
				i2 = i * 4 + 2;
				i3 = i * 4 + 3;

				// UV
				if (textLayer == null) {
					var aSprite = Sprites[cell.Index];
					if (cell.BorderSide == Alignment.Full) {
						// Normal
						uv0 = aSprite.UvBottomLeft;
						uv1 = aSprite.TopLeft;
						uv2 = aSprite.UvTopRight;
						uv3 = aSprite.BottomRight;
					} else {
						// 9 Slice
						aSprite.GetSlicedUvBorder(cell.BorderSide, out var bl, out var br, out var tl, out var tr);
						uv0 = bl;
						uv1 = tl;
						uv2 = tr;
						uv3 = br;
					}
				} else {
					// For Text
					var tSprite = textLayer.CharSprites[cell.Index];
					uv0 = tSprite.UvBottomLeft;
					uv1 = tSprite.UvTopLeft;
					uv2 = tSprite.UvTopRight;
					uv3 = tSprite.UvBottomRight;
				}

				// Shift UV
				if (shifted) {
					if (textLayer == null) {
						uv0.x = uv1.x = Mathf.Lerp(uv0.x, uv3.x, shiftL);
						uv2.x = uv3.x = Mathf.Lerp(uv3.x, uv0.x, shiftR);
						uv0.y = uv3.y = Mathf.Lerp(uv0.y, uv1.y, shiftD);
						uv1.y = uv2.y = Mathf.Lerp(uv1.y, uv0.y, shiftU);
					} else {
						float minUvX = Mathf.Min(Mathf.Min(uv0.x, uv1.x), Mathf.Min(uv2.x, uv3.x));
						float maxUvX = Mathf.Max(Mathf.Max(uv0.x, uv1.x), Mathf.Max(uv2.x, uv3.x));
						float minUvY = Mathf.Min(Mathf.Min(uv0.y, uv1.y), Mathf.Min(uv2.y, uv3.y));
						float maxUvY = Mathf.Max(Mathf.Max(uv0.y, uv1.y), Mathf.Max(uv2.y, uv3.y));
						if (Mathf.Approximately(uv0.x, uv1.x)) {
							uv0.x = uv1.x = Mathf.Lerp(minUvX, maxUvX, shiftL);
							uv2.x = uv3.x = Mathf.Lerp(maxUvX, minUvX, shiftR);
							uv0.y = uv3.y = Mathf.Lerp(maxUvY, minUvY, shiftD);
							uv1.y = uv2.y = Mathf.Lerp(minUvY, maxUvY, shiftU);
						} else {
							uv0.x = uv3.x = Mathf.Lerp(minUvX, maxUvX, shiftD);
							uv2.x = uv1.x = Mathf.Lerp(maxUvX, minUvX, shiftU);
							uv0.y = uv1.y = Mathf.Lerp(maxUvY, minUvY, shiftL);
							uv3.y = uv2.y = Mathf.Lerp(minUvY, maxUvY, shiftR);
						}
					}
				}

				// Pos
				layer.VertexCache[i0] = a;
				layer.VertexCache[i1] = b;
				layer.VertexCache[i2] = c;
				layer.VertexCache[i3] = d;

				// UV
				layer.UvCache[i0] = uv0;
				layer.UvCache[i1] = uv1;
				layer.UvCache[i2] = uv2;
				layer.UvCache[i3] = uv3;

				// Color
				layer.ColorCache[i0] = cell.Color;
				layer.ColorCache[i1] = cell.Color;
				layer.ColorCache[i2] = cell.Color;
				layer.ColorCache[i3] = cell.Color;

			}

			// Clear Unused
			if (cellCount < layer.PrevCellCount) {
				var zero = Vector3.zero;
				for (int i = cellCount; i < layer.PrevCellCount; i++) {
					layer.VertexCache[i * 4 + 0] = zero;
					layer.VertexCache[i * 4 + 1] = zero;
					layer.VertexCache[i * 4 + 2] = zero;
					layer.VertexCache[i * 4 + 3] = zero;
				}
			}
			layer.PrevCellCount = cellCount;

			// Cache >> Mesh
			mesh.SetVertices(layer.VertexCache);
			mesh.SetUVs(0, layer.UvCache);
			mesh.SetColors(layer.ColorCache);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.UploadMeshData(false);
		}


		#endregion




		#region --- API ---


		internal static void BeginDraw (bool isPausing) {
			IsPausing = isPausing;
			IsDrawing = true;
			SetLayerToDefault();
			for (int i = 0; i < Layers.Length; i++) {
				var layer = Layers[i];
				if (!isPausing || layer.UiLayer) {
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


		// Layer
		public static void SetLayer (int index) {
			if (index < 0) {
				CurrentLayerIndex = 1;
			} else {
				CurrentLayerIndex = index.Clamp(0, Layers.Length - 1);
			}
		}
		public static void SetTextLayer (int index) => CurrentTextLayerIndex = index.Clamp(0, TextLayers.Length - 1);
		public static void SetLayerToWallpaper () => CurrentLayerIndex = Const.RENDER_LAYER_WALLPAPER;
		public static void SetLayerToBehind () => CurrentLayerIndex = Const.RENDER_LAYER_BEHIND;
		public static void SetLayerToDefault () => CurrentLayerIndex = Const.RENDER_LAYER_CELL;
		public static void SetLayerToColor () => CurrentLayerIndex = Const.RENDER_LAYER_COLOR;
		public static void SetLayerToMultiply () => CurrentLayerIndex = Const.RENDER_LAYER_MULT;
		public static void SetLayerToAdditive () => CurrentLayerIndex = Const.RENDER_LAYER_ADD;
		public static void SetLayerToUI () => CurrentLayerIndex = Const.RENDER_LAYER_UI;
		public static void SetLayerToTopUI () => CurrentLayerIndex = Const.RENDER_LAYER_TOP_UI;


		public static string GetLayerName (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].RendererRoot.name : "";
		public static string GetTextLayerName () => GetTextLayerName(CurrentTextLayerIndex);
		public static string GetTextLayerName (int layerIndex) => layerIndex >= 0 && layerIndex < TextLayers.Length ? TextLayers[layerIndex].RendererRoot.name : "";


		public static int GetUsedCellCount () => GetUsedCellCount(CurrentLayerIndex);
		public static int GetUsedCellCount (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Count : 0;
		public static int GetTextUsedCellCount () => GetTextUsedCellCount(CurrentTextLayerIndex);
		public static int GetTextUsedCellCount (int layerIndex) => layerIndex >= 0 && layerIndex < TextLayers.Length ? TextLayers[layerIndex].Count : 0;


		public static int GetLayerCapacity (int layerIndex) => layerIndex >= 0 && layerIndex < Layers.Length ? Layers[layerIndex].Cells.Length : 0;
		public static int GetTextLayerCapacity () => GetTextLayerCapacity(CurrentTextLayerIndex);
		public static int GetTextLayerCapacity (int layerIndex) => layerIndex >= 0 && layerIndex < TextLayers.Length ? TextLayers[layerIndex].Cells.Length : 0;


		// Draw
		public static Cell Draw (int globalID, RectInt rect, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, WHITE, z);
		public static Cell Draw (int globalID, RectInt rect, Color32 color, int z = int.MinValue) => Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw(globalID, x, y, pivotX, pivotY, rotation, width, height, WHITE, z);
		public static Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z = int.MinValue) {

			if (!IsDrawing) return EMPTY_CELL;
			bool forText = false;
			if (!SheetIDMap.TryGetValue(globalID, out var rCell)) {
				var tLayer = TextLayers[CurrentTextLayerIndex];
				if (tLayer.TextIDMap.TryGetValue(globalID, out rCell)) {
					forText = true;
				} else {
					return EMPTY_CELL;
				}
			}
			var layer = forText ?
				TextLayers[CurrentTextLayerIndex.Clamp(0, TextLayers.Length - 1)] :
				Layers[CurrentLayerIndex.Clamp(0, Layers.Length - 1)];
			if (IsPausing && !layer.UiLayer) return EMPTY_CELL;
			if (layer.FocusedCell < 0) return EMPTY_CELL;
			var cell = layer.Cells[layer.FocusedCell];

			if (!forText) {

				var sprite = Sprites[rCell.GetIndex(GlobalFrame)];

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
			cell.Shift.x = 0;
			cell.Shift.y = 0;
			cell.Shift.z = 0;
			cell.Shift.w = 0;

			// Final
			layer.FocusedCell++;
			if (layer.FocusedCell >= layer.CellCount) {
				layer.FocusedCell = -1;
			}
			LastDrawnID = globalID;
			return cell;
		}


		public static Cell[] Draw_9Slice (int globalID, RectInt rect) => Draw_9Slice(globalID, rect, WHITE);
		public static Cell[] Draw_9Slice (int globalID, RectInt rect, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color, z);
		public static Cell[] Draw_9Slice (int globalID, RectInt rect, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, RectInt rect, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) => Draw_9Slice(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, borderL, borderR, borderD, borderU, color, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color color, int z = int.MinValue) {
			var border = TryGetSprite(globalID, out var sprite) ? sprite.GlobalBorder : default;
			return Draw_9Slice(
				globalID, x, y, pivotX, pivotY, rotation, width, height,
				border.left, border.right,
				border.down, border.up,
				color, z
			);
		}
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, int z = int.MinValue) => Draw_9Slice(globalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, WHITE, z);
		public static Cell[] Draw_9Slice (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int borderL, int borderR, int borderD, int borderU, Color32 color, int z = int.MinValue) {

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
				if (hasL) {
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
				if (hasM) {
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
				if (hasR) {
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
				if (hasL) {
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
				if (hasM) {
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
				if (hasR) {
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
				if (hasL) {
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
				if (hasM) {
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
				if (hasR) {
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


		public static Cell DrawAnimation (int chainID, RectInt globalRect, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, WHITE, loopStart);
		public static Cell DrawAnimation (int chainID, RectInt globalRect, int frame, Color32 color, int loopStart = int.MinValue) => DrawAnimation(chainID, globalRect.x, globalRect.y, 0, 0, 0, globalRect.width, globalRect.height, frame, color, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int width, int height, int frame, Color32 color, int loopStart = int.MinValue) => DrawAnimation(chainID, x, y, 0, 0, 0, width, height, frame, color, loopStart);
		public static Cell DrawAnimation (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, Color32 color, int loopStart = int.MinValue) {
			if (!SheetIDMap.TryGetValue(chainID, out var rCell)) return EMPTY_CELL;
			int localFrame = GetAnimationFrame(frame, rCell.Length, loopStart == int.MinValue ? rCell.LoopStart : loopStart);
			var sprite = Sprites[rCell.GetIndex(localFrame)];
			return Draw(sprite.GlobalID, x, y, pivotX, pivotY, rotation, width, height, color);
		}


		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int loopStart = -1) => DrawAnimation_9Slice(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, WHITE, loopStart);
		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, Color32 color, int loopStart = -1) {
			if (!SheetIDMap.TryGetValue(chainID, out var rCell)) return Last9SlicedCells;
			var sprite = Sprites[rCell.GetIndex(GetAnimationFrame(frame, rCell.Length, loopStart))];
			return Draw_9Slice(sprite.GlobalID, x, y, pivotX, pivotY, rotation, width, height, sprite.GlobalBorder.left, sprite.GlobalBorder.right, sprite.GlobalBorder.down, sprite.GlobalBorder.up, color);
		}
		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int borderL, int borderR, int borderD, int borderU, int loopStart = -1) => DrawAnimation_9Slice(chainID, x, y, pivotX, pivotY, rotation, width, height, frame, borderL, borderR, borderD, borderU, WHITE, loopStart);
		public static Cell[] DrawAnimation_9Slice (int chainID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, int frame, int borderL, int borderR, int borderD, int borderU, Color32 color, int loopStart = -1) {
			if (!SheetIDMap.TryGetValue(chainID, out var rCell)) return Last9SlicedCells;
			var sprite = Sprites[rCell.GetIndex(GetAnimationFrame(frame, rCell.Length, loopStart))];
			return Draw_9Slice(sprite.GlobalID, x, y, pivotX, pivotY, rotation, width, height, borderL, borderR, borderD, borderU, color);
		}


		// Sprite Data
		public static bool TryGetSprite (int globalID, out AngeSprite sprite) {
			
			if (SheetIDMap.TryGetValue(globalID, out var rCell)) {
				sprite = Sprites[rCell.GetIndex(GlobalFrame)];
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
			if (SpriteGroupMap.TryGetValue(groupID, out var ids)) {
				if (loopIndex) index = index.UMod(ids.Length);
				if (clampIndex) index = index.Clamp(0, ids.Length - 1);
				sprite = null;
				return index >= 0 && index < ids.Length && TryGetSprite(ids[index], out sprite);
			} else return TryGetSprite(groupID, out sprite);
		}


		public static bool TryGetMeta (int globalID, out SpriteMeta meta) => MetaPool.TryGetValue(globalID, out meta);


		public static bool HasSprite (int globalID) => SheetIDMap.ContainsKey(globalID);


		public static AngeSprite GetSpriteAt (int index) => index >= 0 && index < Sprites.Length ? Sprites[index] : null;


		public static int GetSpriteIdAt (int index) => index >= 0 && index < Sprites.Length ? Sprites[index].GlobalID : 0;


		public static AngeSpriteChain GetChainAt (int index) => Chains[index];


		// Cell Effect
		public static void AddEffect (CellEffect effect) {
			for (int i = 0; i < EFFECT_COUNT; i++) {
				if (Effects[i] == null) {
					Effects[i] = effect;
					effect.Start();
					return;
				}
			}
		}


		public static void RemoveEffect<T> () where T : CellEffect {
			for (int i = 0; i < EFFECT_COUNT; i++) {
				if (Effects[i] is T) Effects[i] = null;
			}
		}


		public static bool HasEffect<T> () where T : CellEffect {
			for (int i = 0; i < EFFECT_COUNT; i++) {
				if (Effects[i] is T) return true;
			}
			return false;
		}


		// Misc
		public static void SetBackgroundTint (Color32 top, Color32 bottom) {
			SkyTintTop = top;
			SkyTintBottom = bottom;
			if (Skybox == null) return;
			Skybox.SetColor(SKYBOX_TOP, top);
			Skybox.SetColor(SKYBOX_BOTTOM, bottom);
		}


		public static void StartCameraShake (int duration, int amount = 1000) {
			CameraShakeEndFrame = GlobalFrame + duration.Clamp(0, MAX_CAMERA_SHAKE);
			CameraShakeAmount = amount;
		}


		// Clamp
		public static void ClampTextCells (RectInt rect, int startIndex, int endIndex) => ClampCellsLogic(TextLayers[CurrentTextLayerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampTextCells (int layerIndex, RectInt rect, int startIndex, int endIndex) => ClampCellsLogic(TextLayers[layerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampCells (RectInt rect, int startIndex, int endIndex) => ClampCellsLogic(Layers[CurrentLayerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampCells (int layerIndex, RectInt rect, int startIndex, int endIndex) => ClampCellsLogic(Layers[layerIndex].Cells, rect, startIndex, endIndex);
		public static void ClampCells (Cell[] cells, RectInt rect) => ClampCellsLogic(cells, rect, 0, cells.Length);
		private static void ClampCellsLogic (Cell[] cells, RectInt rect, int startIndex, int endIndex) {
			var cellRect = new RectInt();
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
		internal static bool RequireChar (char c, out CharSprite charSprite) {
			charSprite = null;
			var tLayer = TextLayers[CurrentTextLayerIndex];
			if (tLayer.TextIDMap.TryGetValue(c, out var cInfo)) {
				if (cInfo.Index < 0) {
					// No CharInfo for this Char
					return false;
				} else {
					charSprite = tLayer.CharSprites[cInfo.Index];
					if (charSprite.Rebuild != tLayer.TextRebuild) {
						// Need Cache Again
						if (tLayer.TextFont.GetCharacterInfo(c, out var info, tLayer.TextSize)) {
							float size = info.size == 0 ? tLayer.TextSize : info.size;
							charSprite.GlobalID = info.index;
							charSprite.UvBottomLeft = info.uvBottomLeft;
							charSprite.UvBottomRight = info.uvBottomRight;
							charSprite.UvTopLeft = info.uvTopLeft;
							charSprite.UvTopRight = info.uvTopRight;
							charSprite.Offset = Rect.MinMaxRect(info.minX / size, info.minY / size, info.maxX / size, info.maxY / size);
							charSprite.Advance = info.advance / size;
							charSprite.Rebuild = tLayer.TextRebuild;
						} else {
							cInfo.Index = -1;
						}
					}
					return true;
				}
			} else {
				// Require Char from Font
				if (tLayer.TextFont.GetCharacterInfo(c, out var info, tLayer.TextSize)) {
					// Got Info
					float size = info.size == 0 ? tLayer.TextSize : info.size;

					tLayer.TextIDMap.Add(c, new CellInfo(tLayer.CharSprites.Count));
					tLayer.CharSprites.Add(charSprite = new CharSprite() {
						GlobalID = info.index,
						UvBottomLeft = info.uvBottomLeft,
						UvBottomRight = info.uvBottomRight,
						UvTopLeft = info.uvTopLeft,
						UvTopRight = info.uvTopRight,
						Offset = Rect.MinMaxRect(info.minX / size, info.minY / size, info.maxX / size, info.maxY / size),
						Advance = info.advance / size,
						Rebuild = tLayer.TextRebuild,
					});
					return true;
				} else {
					// No Info
					tLayer.TextIDMap.Add(c, new CellInfo(-1));
					return false;
				}
			}
		}


		internal static void RequestStringForFont (string content) {
			var tLayer = TextLayers[CurrentTextLayerIndex];
			if (tLayer.TextFont == null) return;
			tLayer.TextFont.RequestCharactersInTexture(content, tLayer.TextSize);
		}


		#endregion




	}
}
