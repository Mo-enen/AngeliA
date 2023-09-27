Shader "Angelia/Invert" {
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
			fixed4 frag(v2f_img i) : COLOR{
				return 1 - tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}