using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity {
	public class PostRenderer : MonoBehaviour {




		#region --- SUB ---


		public struct RectCell {
			public RectInt Rect;
			public Color32 Color;
		}


		public struct TextureCell {
			public RectInt Rect;
			public Rect UV;
			public Texture2D Texture;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly string[] EFFECT_SHADER_NAMES = new string[Const.SCREEN_EFFECT_COUNT] {
			"Angelia/ChromaticAberration",
			"Angelia/Tint",
			"Angelia/RetroDarken",
			"Angelia/RetroLighten",
			"Angelia/Vignette",
			"Angelia/GreyScale",
			"Angelia/Invert",
		};
		private static readonly int CA_RED_X = Shader.PropertyToID("_RX");
		private static readonly int CA_RED_Y = Shader.PropertyToID("_RY");
		private static readonly int CA_GREEN_X = Shader.PropertyToID("_GX");
		private static readonly int CA_GREEN_Y = Shader.PropertyToID("_GY");
		private static readonly int CA_BLUE_X = Shader.PropertyToID("_BX");
		private static readonly int CA_BLUE_Y = Shader.PropertyToID("_BY");
		private static readonly int AMOUNT_CODE = Shader.PropertyToID("_Amount");
		private static readonly int TINT_CODE = Shader.PropertyToID("_Tint");
		private static readonly int RADIUS_CODE = Shader.PropertyToID("_Radius");
		private static readonly int FEATHER_CODE = Shader.PropertyToID("_Feather");
		private static readonly int OFFSET_X_CODE = Shader.PropertyToID("_OffsetX");
		private static readonly int OFFSET_Y_CODE = Shader.PropertyToID("_OffsetY");
		private static readonly int ROUND_CODE = Shader.PropertyToID("_Round");

		// Data
		private static readonly RectCell[] Rects = new RectCell[256 * 256];
		private static readonly TextureCell[] Textures = new TextureCell[256];
		private static int RectCount = 0;
		private static int TextureCount = 0;
		private static ScreenEffect[] Effects;
		private Camera Camera;
		private Material RectMaterial;

		// ChromaticAberration
		private static readonly System.Random CA_Ran = new(1928736456);

		#endregion




		#region --- MSG ---


		private void Awake () {
			Camera = GetComponent<Camera>();
			RectMaterial = new Material(Shader.Find("Angelia/Vertex"));
			Effects = new ScreenEffect[Const.SCREEN_EFFECT_COUNT];
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
				var e = Effects[i] = gameObject.AddComponent<ScreenEffect>();
				e.SetEnable(false);
				e.Materal = new Material(Shader.Find(EFFECT_SHADER_NAMES[i]));
			}
		}


		[OnGameUpdate(-4096)]
		public static void OnGameUpdate () {
			RectCount = 0;
			TextureCount = 0;
			for (int i = 0; i < Textures.Length; i++) {
				Textures[i].Texture = null;
			}
			UpdateEffects();
		}


		[OnGameUpdatePauseless]
		public static void OnGameUpdatePauseless () {
			if (Game.IsPausing) {
				RectCount = 0;
				TextureCount = 0;
			}
		}


		private void OnPostRender () {
			if (Camera == null) return;
			var cameraRect = CellRenderer.CameraRect;

			// Textures
			if (TextureCount > 0) {
				int screenWidth = Screen.width;
				int screenHeight = Screen.height;
				var targetCameraRect = Camera.rect;
				targetCameraRect.x *= screenWidth;
				targetCameraRect.width *= screenWidth;
				targetCameraRect.height *= screenHeight;
				GL.LoadPixelMatrix();
				var rect = new Rect();
				for (int i = 0; i < TextureCount; i++) {
					var cell = Textures[i];
					rect.width = Util.RemapUnclamped(
						0, cameraRect.width, 0, targetCameraRect.width, cell.Rect.width
					);
					rect.height = -Util.RemapUnclamped(
						0, cameraRect.height, 0, targetCameraRect.height, cell.Rect.height
					);
					rect.x = Util.RemapUnclamped(
						cameraRect.x, cameraRect.xMax, targetCameraRect.x, targetCameraRect.xMax, cell.Rect.x
					);
					rect.y = Util.RemapUnclamped(
						 cameraRect.y, cameraRect.yMax, targetCameraRect.y, targetCameraRect.yMax, cell.Rect.y
					) - rect.height;
					Graphics.DrawTexture(rect, cell.Texture, cell.UV, 0, 0, 0, 0);
				}
			}

			// Rects
			if (RectCount > 0) {
				RectMaterial.SetPass(0);
				GL.LoadOrtho();
				GL.Begin(GL.QUADS);
				var rect = new Rect();
				for (int i = 0; i < RectCount; i++) {
					try {
						var cell = Rects[i];
						GL.Color(cell.Color);

						rect.x = Util.InverseLerpUnclamped(cameraRect.x, cameraRect.xMax, cell.Rect.x);
						rect.y = Util.InverseLerpUnclamped(cameraRect.y, cameraRect.yMax, cell.Rect.y);
						rect.width = Util.InverseLerpUnclamped(0, cameraRect.width, cell.Rect.width);
						rect.height = Util.InverseLerpUnclamped(0, cameraRect.height, cell.Rect.height);

						GL.Vertex3(rect.x, rect.y, 0.5f);
						GL.Vertex3(rect.x, rect.yMax, 0.5f);
						GL.Vertex3(rect.xMax, rect.yMax, 0.5f);
						GL.Vertex3(rect.xMax, rect.y, 0.5f);

					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
				GL.End();
			}
		}


		private static void UpdateEffects () {
			// Chromatic Aberration
			var ca = Effects[Const.SCREEN_EFFECT_CHROMATIC_ABERRATION];
			float caEnableTime = ca.EnableTime;
			if (caEnableTime > 0f) {
				const float CA_PingPongTime = 0.618f;
				const float CA_PingPongMin = 0f;
				const float CA_PingPongMax = 0.015f;
				var mat = ca.Materal;
				if ((Time.time - caEnableTime) % CA_PingPongTime > CA_PingPongTime / 2f) {
					mat.SetFloat(CA_RED_X, GetRandomAmount(0f, caEnableTime));
					mat.SetFloat(CA_RED_Y, GetRandomAmount(0.2f, caEnableTime));
					mat.SetFloat(CA_BLUE_X, GetRandomAmount(0.7f, caEnableTime));
					mat.SetFloat(CA_BLUE_Y, GetRandomAmount(0.4f, caEnableTime));
				} else {
					mat.SetFloat(CA_GREEN_X, GetRandomAmount(0f, caEnableTime));
					mat.SetFloat(CA_GREEN_Y, GetRandomAmount(0.8f, caEnableTime));
					mat.SetFloat(CA_BLUE_X, GetRandomAmount(0.4f, caEnableTime));
					mat.SetFloat(CA_BLUE_Y, GetRandomAmount(0.72f, caEnableTime));
				}
				static float GetRandomAmount (float timeOffset, float caEnableTime) {
					int range = (int)(Util.RemapUnclamped(
						0f, CA_PingPongTime,
						CA_PingPongMin, CA_PingPongMax,
						Mathf.PingPong(Time.time - caEnableTime + timeOffset * CA_PingPongTime, CA_PingPongTime)
					) * 100000f);
					return CA_Ran.Next(-range, range) / 100000f;
				}
			}
		}


		#endregion




		#region --- API ---


		// Effect
		public static bool GetEffectEnable (int effectIndex) => Effects[effectIndex].enabled;
		public static void SetEffectEnable (int effectIndex, bool enable) => Effects[effectIndex].SetEnable(enable);
		public static void SetDarkenAmount (float amount, float step = 8f) {
			Effects[Const.SCREEN_EFFECT_RETRO_DARKEN].Materal.SetFloat(
				AMOUNT_CODE, (amount * step).RoundToInt() / step
			);
		}
		public static void SetLightenAmount (float amount, float step = 8f) {
			Effects[Const.SCREEN_EFFECT_RETRO_LIGHTEN].Materal.SetFloat(
				AMOUNT_CODE, (amount * step).RoundToInt() / step
			);
		}
		public static void SetTint (Color color) => Effects[Const.SCREEN_EFFECT_TINT].Materal.SetColor(TINT_CODE, color);
		public static void SetVignetteRadius (float radius) => Effects[Const.SCREEN_EFFECT_VIGNETTE].Materal.SetFloat(RADIUS_CODE, radius);
		public static void SetVignetteFeather (float feather) => Effects[Const.SCREEN_EFFECT_VIGNETTE].Materal.SetFloat(FEATHER_CODE, feather);
		public static void SetVignetteOffsetX (float offsetX) => Effects[Const.SCREEN_EFFECT_VIGNETTE].Materal.SetFloat(OFFSET_X_CODE, offsetX);
		public static void SetVignetteOffsetY (float offsetY) => Effects[Const.SCREEN_EFFECT_VIGNETTE].Materal.SetFloat(OFFSET_Y_CODE, offsetY);
		public static void SetVignetteRound (float round) => Effects[Const.SCREEN_EFFECT_VIGNETTE].Materal.SetFloat(ROUND_CODE, round);


		// Gizmos
		public static void DrawRect (RectInt rect, Color32 color) {
			if (RectCount >= Rects.Length) return;
			Rects[RectCount] = new RectCell() { Rect = rect, Color = color, };
			RectCount++;
		}


		public static void DrawTexture (RectInt rect, Texture2D texture) => DrawTexture(rect, new Rect(0, 0, 1, 1), texture);


		public static void DrawTexture (RectInt rect, Rect uv, Texture2D texture) {
			if (TextureCount >= Textures.Length) return;
			Textures[TextureCount] = new TextureCell() { Rect = rect, UV = uv, Texture = texture, };
			TextureCount++;
		}


		#endregion




		#region --- LGC ---





		#endregion















	}
}