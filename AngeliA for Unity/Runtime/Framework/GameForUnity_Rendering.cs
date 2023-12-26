using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace AngeliaForUnity {
	public sealed partial class GameForUnity {




		#region --- SUB ---


		private class RenderingLayerUnity {
			public Transform RendererRoot;
			public MeshRenderer Renderer;
			public Mesh Mesh;
			public List<Vector3> VertexCache;
			public List<Vector2> UvCache;
			public List<Color32> ColorCache;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly Shader[] RENDERING_SHADERS = new Shader[RenderLayer.COUNT] {
			Shader.Find("Angelia/Lerp"),	// Wallpaper
			Shader.Find("Angelia/Lerp"),	// Behind
			Shader.Find("Angelia/Color"),	// Shadow
			Shader.Find("Angelia/Cell"),	// Default
			Shader.Find("Angelia/Color"),	// Color
			Shader.Find("Angelia/Mult"),	// Mult
			Shader.Find("Angelia/Add"),		// Add
			Shader.Find("Angelia/Cell"),	// UI
			Shader.Find("Angelia/Cell"),	// TopUI
		};
		private static readonly int SKYBOX_TOP = Shader.PropertyToID("_ColorA");
		private static readonly int SKYBOX_BOTTOM = Shader.PropertyToID("_ColorB");
		private static readonly Shader SKYBOX_SHADER = Shader.Find("Angelia/Skybox");

		// Data
		private RenderingLayerUnity[] RenderingLayers = new RenderingLayerUnity[0];
		private RenderingLayerUnity[] RenderingTextLayers = new RenderingLayerUnity[0];
		private Texture2D SheetTexture = null;
		private Material Skybox = null;


		#endregion




		#region --- MSG ---


		private void InitializeRendering () {

			var root = UnityCamera.transform;
			root.DestroyAllChildrenImmediate();
			SheetTexture = AngeUtilUnity.LoadTexture(AngePath.SheetTexturePath);
			if (SKYBOX_SHADER != null) {
				RenderSettings.skybox = Skybox = new Material(SKYBOX_SHADER);
			}

			// Layers
			RenderingLayers = new RenderingLayerUnity[RenderLayer.COUNT];
			RenderingTextLayers = new RenderingLayerUnity[Fonts.Length];
			for (int i = 0; i < RenderingTextLayers.Length; i++) {
				int layerIndex = i;
				var tLayer = RenderingTextLayers[layerIndex];
				Font.textureRebuilt += (_font) => {
					if (_font == Fonts[layerIndex]) CellRenderer.AddTextRebuild(layerIndex);
				};
			}
		}


		#endregion




		#region --- API ---


		// Camera
		protected override FRect _GetCameraScreenLocacion () => UnityCamera.rect;
		protected override void _SetCameraScreenLocacion (FRect rect) => UnityCamera.rect = rect;
		protected override float _GetCameraAspect () => UnityCamera.aspect;
		protected override float _GetCameraOrthographicSize () => UnityCamera.orthographicSize;

		// Renderer
		protected override void _OnCameraUpdate () {

			float orthographicSize = CameraOrthographicSize;
			float aspect = CameraAspect;
			var pos = new Float3(
				-orthographicSize * aspect,
				-orthographicSize,
				1f
			);
			var cameraRect = CellRenderer.CameraRect;
			var viewRect = CellRenderer.ViewRect;
			pos.x -= ((viewRect.width - cameraRect.width) / 2) * orthographicSize * 2f * aspect / cameraRect.width;
			var scl = new Float3(
				orthographicSize * 2f / viewRect.height,
				orthographicSize * 2f / viewRect.height,
				1f
			);

			var rLayers = InstanceUnity.RenderingLayers;
			var tLayers = InstanceUnity.RenderingTextLayers;
			for (int layerIndex = 0; layerIndex < rLayers.Length; layerIndex++) {
				var layer = rLayers[layerIndex];
				layer.RendererRoot.localPosition = pos;
				layer.RendererRoot.localScale = scl;
			}
			for (int layerIndex = 0; layerIndex < tLayers.Length; layerIndex++) {
				var tLayer = tLayers[layerIndex];
				tLayer.RendererRoot.localPosition = pos;
				tLayer.RendererRoot.localScale = scl;
			}

		}

		protected override void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) {
			RenderingLayers[index] = CreateRenderingLayer(
				UnityCamera.transform,
				new Material(RENDERING_SHADERS[index]) {
					name = name,
					mainTexture = SheetTexture,
					enableInstancing = true,
					mainTextureOffset = Float2.zero,
					mainTextureScale = Float2.one,
					doubleSidedGI = false,
					renderQueue = 3000,
				},
				name,
				sortingOrder,
				capacity
			);
		}
		protected override void _OnTextLayerCreated (int index, string name, int sortingOrder, int capacity) {
			RenderingTextLayers[index] = CreateRenderingLayer(
				UnityCamera.transform,
				Fonts[index].material,
				name,
				sortingOrder,
				capacity
			);
		}

		protected override int _GetTextLayerCount () => Fonts.Length;
		protected override string _GetTextLayerName (int index) => Fonts[index].name;
		protected override int _GetFontSize (int index) => Fonts[index].fontSize;

		protected override void _OnLayerUpdate (int layerIndex, bool isTextLayer, Cell[] cells, int cellCount, ref int prevCellCount) {

			var viewRect = CellRenderer.ViewRect;
			Float3 a = Float3.zero;
			Float3 b = Float3.zero;
			Float3 c = Float3.zero;
			Float3 d = Float3.zero;
			Float2 uv0;
			Float2 uv1;
			Float2 uv2;
			Float2 uv3;
			int i0, i1, i2, i3;
			float shiftL;
			float shiftR;
			float shiftD;
			float shiftU;
			var layer = isTextLayer ? RenderingTextLayers[layerIndex] : RenderingLayers[layerIndex];

			for (int i = 0; i < cellCount; i++) {

				var cell = cells[i];

				if (cell.Index < 0) continue;
				int indexCount = isTextLayer ? CellRenderer.GetCharSpriteCount(layerIndex) : CellRenderer.SpriteCount;
				if (cell.Index >= indexCount) continue;

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
					a.x = b.x = Util.Lerp(a.x, d.x, shiftL);
					c.x = d.x = Util.Lerp(d.x, a.x, shiftR);
					a.y = d.y = Util.Lerp(a.y, b.y, shiftD);
					b.y = c.y = Util.Lerp(b.y, a.y, shiftU);
				}

				// Rotation
				if (cell.Rotation != 0) {
					a = a.Rotate(cell.Rotation);
					b = b.Rotate(cell.Rotation);
					c = c.Rotate(cell.Rotation);
					d = d.Rotate(cell.Rotation);
				}

				// Global to View
				a.x += cell.X - viewRect.x;
				a.y += cell.Y - viewRect.y;
				b.x += cell.X - viewRect.x;
				b.y += cell.Y - viewRect.y;
				c.x += cell.X - viewRect.x;
				c.y += cell.Y - viewRect.y;
				d.x += cell.X - viewRect.x;
				d.y += cell.Y - viewRect.y;

				i0 = i * 4 + 0;
				i1 = i * 4 + 1;
				i2 = i * 4 + 2;
				i3 = i * 4 + 3;

				// UV
				if (!isTextLayer) {
					var aSprite = CellRenderer.GetSpriteAt(cell.Index);
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
					var tSprite = CellRenderer.GetCharSprite(layerIndex, cell.Index);
					uv0 = tSprite.UvBottomLeft;
					uv1 = tSprite.UvTopLeft;
					uv2 = tSprite.UvTopRight;
					uv3 = tSprite.UvBottomRight;
				}

				// Shift UV
				if (shifted) {
					if (!isTextLayer) {
						uv0.x = uv1.x = Util.Lerp(uv0.x, uv3.x, shiftL);
						uv2.x = uv3.x = Util.Lerp(uv3.x, uv0.x, shiftR);
						uv0.y = uv3.y = Util.Lerp(uv0.y, uv1.y, shiftD);
						uv1.y = uv2.y = Util.Lerp(uv1.y, uv0.y, shiftU);
					} else {
						float minUvX = Util.Min(Util.Min(uv0.x, uv1.x), Util.Min(uv2.x, uv3.x));
						float maxUvX = Util.Max(Util.Max(uv0.x, uv1.x), Util.Max(uv2.x, uv3.x));
						float minUvY = Util.Min(Util.Min(uv0.y, uv1.y), Util.Min(uv2.y, uv3.y));
						float maxUvY = Util.Max(Util.Max(uv0.y, uv1.y), Util.Max(uv2.y, uv3.y));
						if (Util.Approximately(uv0.x, uv1.x)) {
							uv0.x = uv1.x = Util.Lerp(minUvX, maxUvX, shiftL);
							uv2.x = uv3.x = Util.Lerp(maxUvX, minUvX, shiftR);
							uv0.y = uv3.y = Util.Lerp(maxUvY, minUvY, shiftD);
							uv1.y = uv2.y = Util.Lerp(minUvY, maxUvY, shiftU);
						} else {
							uv0.x = uv3.x = Util.Lerp(minUvX, maxUvX, shiftD);
							uv2.x = uv1.x = Util.Lerp(maxUvX, minUvX, shiftU);
							uv0.y = uv1.y = Util.Lerp(maxUvY, minUvY, shiftL);
							uv3.y = uv2.y = Util.Lerp(minUvY, maxUvY, shiftR);
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
			if (cellCount < prevCellCount) {
				var zero = Vector3.zero;
				for (int i = cellCount; i < prevCellCount; i++) {
					layer.VertexCache[i * 4 + 0] = zero;
					layer.VertexCache[i * 4 + 1] = zero;
					layer.VertexCache[i * 4 + 2] = zero;
					layer.VertexCache[i * 4 + 3] = zero;
				}
			}
			prevCellCount = cellCount;

			// Cache >> Mesh
			var mesh = layer.Mesh;
			mesh.SetVertices(layer.VertexCache);
			mesh.SetUVs(0, layer.UvCache);
			mesh.SetColors(layer.ColorCache);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.UploadMeshData(false);
		}

		protected override CellRenderer.CharSprite _FillCharSprite (int layerIndex, char c, int textSize, int rebuildVersion, CellRenderer.CharSprite charSprite, out bool filled) {
			var font = Fonts[layerIndex];
			if (font.GetCharacterInfo(c, out var info, textSize)) {
				float size = info.size == 0 ? textSize : info.size;
				charSprite ??= new();
				charSprite.GlobalID = info.index;
				charSprite.UvBottomLeft = info.uvBottomLeft.ToAngelia();
				charSprite.UvBottomRight = info.uvBottomRight.ToAngelia();
				charSprite.UvTopLeft = info.uvTopLeft.ToAngelia();
				charSprite.UvTopRight = info.uvTopRight.ToAngelia();
				charSprite.Offset = FRect.MinMaxRect(info.minX / size, info.minY / size, info.maxX / size, info.maxY / size);
				charSprite.Advance = info.advance / size;
				charSprite.Rebuild = rebuildVersion;
				filled = true;
				return charSprite;
			}
			filled = false;
			return charSprite;
		}

		protected override void _RequestStringForFont (int layerIndex, int textSize, string content) => Fonts[layerIndex].RequestCharactersInTexture(content, textSize);

		protected override void _RequestStringForFont (int layerIndex, int textSize, char[] content) {
			var font = Fonts[layerIndex];
			if (font == null) return;
			for (int i = 0; i < content.Length; i++) {
				char c = content[i];
				if (font.HasCharacter(c) && !font.GetCharacterInfo(c, out _, textSize)) {
					font.RequestCharactersInTexture(c.ToString(), textSize);
				}
			}
		}

		protected override void _SetSkyboxTint (Byte4 top, Byte4 bottom) {
			if (Skybox == null) return;
			Skybox.SetColor(SKYBOX_TOP, top);
			Skybox.SetColor(SKYBOX_BOTTOM, bottom);
		}

		#endregion




		#region --- LGC ---


		private static Camera GetOrCreateCamera () {
			var gameCamera = Camera.main;
			if (gameCamera == null) {
				var rendererRoot = new GameObject("Renderer", typeof(Camera)).transform;
				rendererRoot.SetParent(null);
				rendererRoot.tag = "MainCamera";
				gameCamera = rendererRoot.GetComponent<Camera>();
			}
			gameCamera.transform.SetPositionAndRotation(Float3.zero, default);
			gameCamera.transform.localScale = Float3.one;
			gameCamera.transform.gameObject.tag = "MainCamera";
			gameCamera.clearFlags = CameraClearFlags.Skybox;
			gameCamera.backgroundColor = new Byte4(0, 0, 0, 0);
			gameCamera.cullingMask = -1;
			gameCamera.orthographic = true;
			gameCamera.orthographicSize = 1f;
			gameCamera.nearClipPlane = 0f;
			gameCamera.farClipPlane = 1024f;
			gameCamera.rect = new FRect(0f, 0f, 1f, 1f);
			gameCamera.depth = 0f;
			gameCamera.renderingPath = RenderingPath.UsePlayerSettings;
			gameCamera.useOcclusionCulling = false;
			gameCamera.allowHDR = false;
			gameCamera.allowMSAA = false;
			gameCamera.allowDynamicResolution = false;
			gameCamera.targetDisplay = 0;
			gameCamera.enabled = true;
			gameCamera.gameObject.SetActive(true);
			return gameCamera;
		}


		private static RenderingLayerUnity CreateRenderingLayer (Transform root, Material material, string name, int sortingOrder, int renderCapacity) {

			if (material == null) {
				material = new Material(Shader.Find("Angelia/Cell"));
			}

			var tf = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)).transform;
			tf.SetParent(root);
			tf.SetAsLastSibling();
			tf.SetPositionAndRotation(new Float3(0, 0, 1), default);
			tf.localScale = Float3.one;
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

			// Create Layer
			var layer = new RenderingLayerUnity {
				Mesh = filter.sharedMesh,
				RendererRoot = filter.transform,
				VertexCache = new(),
				UvCache = new(),
				ColorCache = new(),
				Renderer = mr,
			};

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


		#endregion




	}
}