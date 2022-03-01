//#define FIX_WATER_MARK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	public static class CellRenderer {




		#region --- SUB ---



		public struct Cell {
			public int ID;
			public int X;
			public int Y;
			public int Width;
			public int Height;
			public int Rotation;
			public int PivotX;
			public int PivotY;
			public Color32 Color;
		}



		public class Layer {
			public UvRect[] UVs;
			public Cell[] Cells;
			public int CellCount;
			public int UVCount;
			public int FocusedCell;
			public int PrevCellCount;
			public Transform RendererRoot;
			public Mesh Mesh;
			public List<Vector3> VertexCache;
			public List<Vector2> UvCache;
			public List<Color32> ColorCache;
		}



		public class CharLayer : Layer {
			public Rect[] UvOffsets;
			public bool[] FullWidths;
		}



		public delegate void VoidHandler ();



		#endregion




		#region --- VAR ---


		// Const
		private static Cell EMPTY_CELL = new() { ID = -1 };

		// Api
		public static RectInt ViewRect { get; set; } = default;
		public static RectInt CameraRect { get; private set; } = default;
		public static event VoidHandler BeforeUpdate = null;

		// Data
		private static Layer[] Layers = new Layer[0];
		private static Layer FocusedLayer = null;
		private static CharLayer CharacterLayer = null;
		private static Dictionary<int, (int sheet, int id)> SheetIDMap = new();
		private static Camera MainCamera = null;
		private static int FocusedLayerIndex = -1;


		#endregion




		#region --- MSG ---


		public static void Initialize (SpriteSheet[] sheets) {

			int layerCount = sheets.Length;
			Layers = new Layer[layerCount];
			SheetIDMap.Clear();

			var rendererRoot = new GameObject("Renderer", typeof(Camera)).transform;
			rendererRoot.SetParent(null);
			rendererRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			rendererRoot.localScale = Vector3.one;
			var camera = MainCamera = rendererRoot.GetComponent<Camera>();
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = new Color32(34, 34, 34, 0);
			camera.cullingMask = -1;
			camera.orthographic = true;
			camera.orthographicSize = 1f;
			camera.nearClipPlane = 0f;
			camera.farClipPlane = 2f;
			camera.rect = new Rect(0f, 0f, 1f, 1f);
			camera.depth = 0f;
			camera.renderingPath = RenderingPath.UsePlayerSettings;
			camera.useOcclusionCulling = false;
			camera.allowHDR = false;
			camera.allowMSAA = false;
			camera.allowDynamicResolution = false;
			camera.targetDisplay = 0;
			for (int i = 0; i < sheets.Length; i++) {
				var sheet = sheets[i];
				// Mesh Renderer
				var tf = new GameObject(sheet.name, typeof(MeshFilter), typeof(MeshRenderer)).transform;
				tf.SetParent(rendererRoot);
				tf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				tf.localScale = Vector3.one;
				var mf = tf.GetComponent<MeshFilter>();
				var mr = tf.GetComponent<MeshRenderer>();
				mf.sharedMesh = new Mesh() { name = sheet.name, };
				mr.material = sheet.GetMaterial();
				mr.receiveShadows = false;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				mr.staticShadowCaster = false;
				mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
				mr.sortingOrder = i;
				// Renderer
				SetupLayer(i, sheet, mf);
			}

#if FIX_WATER_MARK
			{
				// Water Mark
				var tf = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				try {
					tf.name = "Water Mark";
					tf.SetParent(rendererRoot);
					tf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
					tf.localScale = Vector3.one;
					var mf = tf.GetComponent<MeshFilter>();
					var mr = tf.GetComponent<MeshRenderer>();
					Object.DestroyImmediate(tf.gameObject.GetComponent<MeshCollider>(), false);
					mr.sortingOrder = sheets.Length;
					mr.material = new Material(Shader.Find("Cell"));
					mr.receiveShadows = false;
					mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					mr.staticShadowCaster = false;
					mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
					RefreshWaterMarkPosition(tf);
				} catch (System.Exception ex) {
					if (tf != null) {
						Object.DestroyImmediate(tf.gameObject, false);
					}
					Debug.LogException(ex);
				}
			}
#endif
		}


		public static void FrameUpdate () {

			BeforeUpdate?.Invoke();

			// Ratio
			float ratio = (float)Screen.width / Screen.height;
			float maxRatio = (float)ViewRect.width / ViewRect.height;
			var rect = new Rect(0f, 0f, 1f, 1f);
			if (ratio > maxRatio) {
				rect = new Rect(0.5f - 0.5f * maxRatio / ratio, 0f, maxRatio / ratio, 1f);
			}
			if (MainCamera.rect.NotAlmost(rect)) {
				MainCamera.rect = rect;
			}

			// Camera Rect
			var cRect = new RectInt(
				ViewRect.x,
				ViewRect.y,
				(int)(ViewRect.height * MainCamera.aspect),
				ViewRect.height
			);
			int cOffsetX = (ViewRect.width - CameraRect.width) / 2;
			cRect.x += cOffsetX;
			CameraRect = cRect;

			// Render
			int layerCount = Layers.Length;
			var pos = new Vector3(
				-MainCamera.orthographicSize * MainCamera.aspect,
				-MainCamera.orthographicSize,
				0f
			);
			pos.x -= cOffsetX * MainCamera.orthographicSize * 2f * MainCamera.aspect / cRect.width;
			var scl = new Vector3(
				MainCamera.orthographicSize * 2f / ViewRect.height,
				MainCamera.orthographicSize * 2f / ViewRect.height,
				1f
			);
			for (int index = 0; index < layerCount; index++) {
				var layer = Layers[index];
				layer.RendererRoot.localPosition = pos;
				layer.RendererRoot.localScale = scl;
				UpdateLayer(layer);
			}
		}


		private static void UpdateLayer (Layer layer) {

			var mesh = layer.Mesh;

			var cells = layer.Cells;
			int drawCount = layer.FocusedCell >= 0 ? layer.FocusedCell : cells.Length;
			int cellCount = Mathf.Min(cells.Length, drawCount);
			var uvs = layer.UVs;

			var a = Vector3.zero;
			var b = Vector3.zero;
			var c = Vector3.zero;
			var d = Vector3.zero;

			for (int i = 0; i < cellCount; i++) {

				var cell = cells[i];

				if (cell.ID < 0) { continue; }

				// Position
				float pX = cell.Width * (cell.PivotX / 1000f);
				float pY = cell.Height * (cell.PivotY / 1000f);
				a.x = -pX;
				a.y = -pY;
				b.x = -pX;
				b.y = cell.Height - pY;
				c.x = cell.Width - pX;
				c.y = cell.Height - pY;
				d.x = cell.Width - pX;
				d.y = -pY;

				// Rotation
				if (cell.Rotation != 0) {
					var rot = Quaternion.Euler(0, 0, -cell.Rotation);
					a = rot * a;
					b = rot * b;
					c = rot * c;
					d = rot * d;
				}

				a.x += cell.X - ViewRect.x;
				a.y += cell.Y - ViewRect.y;
				b.x += cell.X - ViewRect.x;
				b.y += cell.Y - ViewRect.y;
				c.x += cell.X - ViewRect.x;
				c.y += cell.Y - ViewRect.y;
				d.x += cell.X - ViewRect.x;
				d.y += cell.Y - ViewRect.y;

				// Final
				layer.VertexCache[i * 4 + 0] = a;
				layer.VertexCache[i * 4 + 1] = b;
				layer.VertexCache[i * 4 + 2] = c;
				layer.VertexCache[i * 4 + 3] = d;

				var uv = uvs[cell.ID];
				layer.UvCache[i * 4 + 0] = uv.BottomLeft;
				layer.UvCache[i * 4 + 1] = uv.TopLeft;
				layer.UvCache[i * 4 + 2] = uv.TopRight;
				layer.UvCache[i * 4 + 3] = uv.BottomRight;

				layer.ColorCache[i * 4 + 0] = cell.Color;
				layer.ColorCache[i * 4 + 1] = cell.Color;
				layer.ColorCache[i * 4 + 2] = cell.Color;
				layer.ColorCache[i * 4 + 3] = cell.Color;

			}

			// Clear Unsed
			if (cellCount < layer.PrevCellCount) {
				var zero = Vector3.zero;
				for (int i = cellCount; i < layer.PrevCellCount; i++) {
					try {
						layer.VertexCache[i * 4 + 0] = zero;
					} catch {
						Debug.Log((i * 4) + " " + layer.VertexCache.Count);
					}
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


		// Draw
		public static void BeginDraw () {
			for (int i = 0; i < Layers.Length; i++) {
				BeginDraw(i);
			}
		}


		public static void BeginDraw (int layerIndex) => Layers[layerIndex].FocusedCell = 0;


		public static void BeginCharacterDraw () => CharacterLayer.FocusedCell = 0;


		public static ref Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height) => ref Draw(
			globalID, x, y, pivotX, pivotY, rotation, width, height, new Color32(255, 255, 255, 255)
		);


		public static ref Cell Draw (int globalID, RectInt rect) => ref Draw(globalID, rect, new Color32(255, 255, 255, 255));


		public static ref Cell Draw (int globalID, RectInt rect, Color32 color) => ref Draw(globalID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, color);


		public static ref Cell Draw (int globalID, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color) {
			if (!SheetIDMap.ContainsKey(globalID)) { return ref EMPTY_CELL; }
			int sheet = SheetIDMap[globalID].sheet;
			if (sheet != FocusedLayerIndex) {
				FocusedLayerIndex = sheet;
				FocusedLayer = Layers[sheet];
			}
			if (FocusedLayer.FocusedCell < 0) { return ref EMPTY_CELL; }
			ref var cell = ref FocusedLayer.Cells[FocusedLayer.FocusedCell];
			cell.ID = SheetIDMap[globalID].id;
			cell.X = x;
			cell.Y = y;
			cell.Width = width;
			cell.Height = height;
			cell.Rotation = rotation;
			cell.PivotX = pivotX;
			cell.PivotY = pivotY;
			cell.Color = color;
			FocusedLayer.FocusedCell++;
			if (FocusedLayer.FocusedCell >= FocusedLayer.CellCount) {
				FocusedLayer.FocusedCell = -1;
			}
			return ref cell;
		}


		public static void DrawChar (int globalID, int x, int y, int width, int height, Color32 color, out bool fullWidth) {
			fullWidth = false;
			ref var cell = ref Draw(globalID, x, y, 0, 0, 0, width, height, color);
			if (cell.ID < 0) { return; }
			var uvOffset = CharacterLayer.UvOffsets[cell.ID];
			fullWidth = CharacterLayer.FullWidths[cell.ID];
			cell.X += (int)(cell.Width * uvOffset.x);
			cell.Y += (int)(cell.Height * uvOffset.y);
			cell.Width = (int)(cell.Width * uvOffset.width);
			cell.Height = (int)(cell.Height * uvOffset.height);
		}


		// Misc
		public static bool IsFullWidth (int charID) {
			if (SheetIDMap.ContainsKey(charID)) {
				return CharacterLayer.FullWidths[SheetIDMap[charID].id];
			}
			return false;
		}


		public static bool GetUVRect (int globalID, out UvRect rect) {
			rect = default;
			if (!SheetIDMap.ContainsKey(globalID)) return false;
			var (sheet, id) = SheetIDMap[globalID];
			rect = Layers[sheet].UVs[id];
			return true;
		}


		#endregion




		#region --- LGC ---


		private static void SetupLayer (int layerIndex, SpriteSheet sheet, MeshFilter filter) {
			int cellCapaticy = sheet.RendererCapacity;
			var uvs = sheet.GetUVs();
			var sprites = sheet.Sprites;
			var cells = new Cell[cellCapaticy];
			for (int i = 0; i < cellCapaticy; i++) {
				cells[i] = new Cell() { ID = -1 };
			}
			// Layer
			Layer layer = null;
			if (sheet is SpriteSheetChar) {
				layer = CharacterLayer = new CharLayer();
			} else {
				layer = new Layer();
			}
			layer.Cells = cells;
			layer.UVs = uvs;
			layer.Mesh = filter.sharedMesh;
			layer.RendererRoot = filter.transform;
			layer.UVCount = uvs.Length;
			layer.CellCount = cellCapaticy;
			layer.FocusedCell = 0;
			layer.VertexCache = new();
			layer.UvCache = new();
			layer.ColorCache = new();
			layer.PrevCellCount = 0;

			Layers[layerIndex] = FocusedLayer = layer;
			FocusedLayerIndex = layerIndex;
			for (int i = 0; i < sprites.Length; i++) {
				var sp = sprites[i];
				int id = sp.GlobalID;
				if (!SheetIDMap.ContainsKey(id)) {
					SheetIDMap.Add(id, (layerIndex, i));
				}
#if UNITY_EDITOR
				else {
					Debug.LogError($"[Cell Renderer] Sprite id already exists.(sheet:{sheet.name}, index:{i})");
				}
#endif
			}
			// Init Mesh
			layer.VertexCache.AddRange(new Vector3[cellCapaticy * 4]);
			layer.UvCache.AddRange(new Vector2[cellCapaticy * 4]);
			layer.ColorCache.AddRange(new Color32[cellCapaticy * 4]);
			var mesh = filter.sharedMesh;
			mesh.MarkDynamic();
			mesh.SetVertices(layer.VertexCache);
			mesh.SetUVs(0, layer.UvCache);
			mesh.SetColors(layer.ColorCache);
			mesh.SetTriangles(GetTriangles(cellCapaticy), 0);
			mesh.UploadMeshData(false);
			// Init Char
			if (sheet is SpriteSheetChar cSheet) {
				var cLayer = layer as CharLayer;
				cLayer.UvOffsets = new Rect[cSheet.CharSprites.Length];
				cLayer.FullWidths = new bool[cSheet.CharSprites.Length];
				for (int i = 0; i < cSheet.CharSprites.Length; i++) {
					var sp = cSheet.CharSprites[i];
					cLayer.UvOffsets[i] = sp.UvOffset;
					cLayer.FullWidths[i] = sp.FullWidth;
				}
			}
		}


		private static int[] GetTriangles (int cellCount) {
			var tris = new int[cellCount * 2 * 3];
			for (int i = 0; i < cellCount; i++) {
				tris[i * 6 + 0] = i * 4 + 0;
				tris[i * 6 + 1] = i * 4 + 1;
				tris[i * 6 + 2] = i * 4 + 2;
				tris[i * 6 + 3] = i * 4 + 0;
				tris[i * 6 + 4] = i * 4 + 2;
				tris[i * 6 + 5] = i * 4 + 3;
			}
			return tris;
		}


		private static void RefreshWaterMarkPosition (Transform tf) {
			float width = 0.1f;
			float height = 0.1f;
			tf.localScale = new Vector3(width, height, 1f);
			tf.localPosition = new Vector3(
				MainCamera.orthographicSize * MainCamera.aspect - width / 2f,
				-MainCamera.orthographicSize + height / 2,
				0
			);
		}


		#endregion




	}
}
