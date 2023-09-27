Shader "Angelia/GreyScale" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
			Pass {
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				uniform sampler2D _MainTex;
				fixed4 frag(v2f_img i) : COLOR {
					fixed4 col = tex2D(_MainTex, i.uv);
					return 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
				}
				ENDCG
			}
	}
}