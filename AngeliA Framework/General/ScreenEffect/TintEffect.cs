using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class TintEffect : AngeliaScreenEffect {

		public static readonly int TYPE_ID = typeof(TintEffect).AngeHash();
		private static readonly int TINT_CODE = Shader.PropertyToID("_Tint");

		public override Shader GetShader () => Shader.Find("Angelia/Tint");
		public override int Order => 3;

		public static void SetTint (Color color) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetColor(TINT_CODE, color);
		}

		public static Color GetTint () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetColor(TINT_CODE);
			return Color.white;
		}

	}
}