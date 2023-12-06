using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class RetroDarkenEffect : AngeliaScreenEffect {


		// Const
		public static readonly int TYPE_ID = typeof(RetroDarkenEffect).AngeHash();
		private static readonly int AMOUNT_CODE = Shader.PropertyToID("_Amount");

		// Api
		public override int Order => 4;
		public override Shader GetShader () => Shader.Find("Angelia/RetroDarken");

		// API
		public static void SetAmount (float amount, float step = 8f) {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) mat.SetFloat(AMOUNT_CODE, (amount * step).RoundToInt() / step);
		}
		public static float GetAmount () {
			var mat = ScreenEffect.GetEffectMaterial(TYPE_ID);
			if (mat != null) return mat.GetFloat(AMOUNT_CODE);
			return 0f;
		}


	}
}