Shader "Angelia/Vignette" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Radius("Radius", Range(0.0, 1.0)) = 1.0
		_Feather("Feather", Range(0.0, 1.0)) = 1.0
		_OffsetX("Offset X", Range(-1.0, 1.0)) = 0.0
		_OffsetY("Offset Y", Range(-1.0, 1.0)) = 0.0
		_Round("Round", Range(0.0, 1.0)) = 0.0
	}

		SubShader{
			Pass {
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				uniform sampler2D _MainTex;


				fixed _Radius, _Feather, _OffsetX, _OffsetY, _Round;

				fixed4 frag(v2f_img i) : COLOR{

					fixed4 col = tex2D(_MainTex, i.uv);
					fixed2 newUV = i.uv * 2 - 1;

					newUV.x -= _OffsetX;
					newUV.y -= _OffsetY;

					fixed2 round = fixed2(lerp(1, unity_OrthoParams.x / unity_OrthoParams.y, _Round), 1);

					fixed circle = length(newUV * round);
					fixed mask = 1 - smoothstep(_Radius, _Radius + _Feather, circle);
					
					fixed3 displayColor = col.rgb * mask;

					return fixed4(displayColor, 1);
				}
				ENDCG
			}
		}
}