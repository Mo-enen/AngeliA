Shader "Angelia/RetroDarken" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Amount("Amount", Range(0.0, 1.0)) = 0
	}
		SubShader{
			Pass {
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				uniform sampler2D _MainTex;
				fixed _Amount;
				fixed4 frag(v2f_img i) : COLOR{
					fixed4 col = tex2D(_MainTex, i.uv);
					fixed lum =  (0.299 * col.r + 0.587 * col.g + 0.114 * col.b);
					if (lum < _Amount) {
						return lerp(col, 0, _Amount );
					} else {
						return lerp(col, 0, _Amount * _Amount);
					}
				}
				ENDCG
			}
		}
}