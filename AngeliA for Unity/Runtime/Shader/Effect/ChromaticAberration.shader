Shader "Angelia/ChromaticAberration" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}

		_RX("rx", Range(-0.5, 0.5)) = 0.0
		_RY("ry", Range(-0.5, 0.5)) = 0.0
		_GX("gx", Range(-0.5, 0.5)) = 0.0
		_GY("gy", Range(-0.5, 0.5)) = 0.0
		_BX("bx", Range(-0.5, 0.5)) = 0.0
		_BY("by", Range(-0.5, 0.5)) = 0.0

	}
	SubShader{
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			float _RX, _RY, _GX, _GY, _BX, _BY;
			fixed4 frag(v2f_img i) : COLOR {
				return fixed4(
					tex2D(_MainTex, i.uv + float2(_RX, _RY)).r,
					tex2D(_MainTex, i.uv + float2(_GX, _GY)).g,
					tex2D(_MainTex, i.uv + float2(_BX, _BY)).b,
					1
				);
			}
			ENDCG
		}
	}
}