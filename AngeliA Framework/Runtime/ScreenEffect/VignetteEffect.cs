using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class VignetteEffect : AngeliaScreenEffect {

		public static readonly int TYPE_ID = typeof(VignetteEffect).AngeHash();
		private static readonly int RADIUS_CODE = Shader.PropertyToID("_Radius");
		private static readonly int FEATHER_CODE = Shader.PropertyToID("_Feather");
		private static readonly int OFFSET_X_CODE = Shader.PropertyToID("_OffsetX");
		private static readonly int OFFSET_Y_CODE = Shader.PropertyToID("_OffsetY");
		private static readonly int ROUND_CODE = Shader.PropertyToID("_Round");

		public override int Order => 2;
		public override Shader GetShader () => Shader.Find("Angelia/Vignette");

		public static void SetRadius (float radius) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetFloat(RADIUS_CODE, radius);
		}
		public static void SetFeather (float feather) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetFloat(FEATHER_CODE, feather);
		}
		public static void SetOffsetX (float offsetX) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetFloat(OFFSET_X_CODE, offsetX);
		}
		public static void SetOffsetY (float offsetY) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetFloat(OFFSET_Y_CODE, offsetY);
		}
		public static void SetRound (float round) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetFloat(ROUND_CODE, round);
		}

		public static float GetRadius () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetFloat(RADIUS_CODE);
			return 0f;
		}
		public static float GetFeather () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetFloat(FEATHER_CODE);
			return 0f;
		}
		public static float GetOffsetX () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetFloat(OFFSET_X_CODE);
			return 0f;
		}
		public static float GetOffsetY () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetFloat(OFFSET_Y_CODE);
			return 0f;
		}
		public static float GetRound () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetFloat(ROUND_CODE);
			return 0f;
		}


	}
}