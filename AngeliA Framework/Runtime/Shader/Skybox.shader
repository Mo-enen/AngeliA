Shader "Angelia/Skybox" {
	Properties{
		_ColorA("Top", Color) = (1,1,1,1)
		_ColorB("Bottom", Color) = (0,0,0,0)
	}

		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma target 2.0
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						float4 screenPos : TEXCOORD1;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					fixed4 _ColorA, _ColorB;

					v2f vert(appdata_t v) {
						v2f o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.screenPos = ComputeScreenPos(o.vertex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target {
						return lerp(_ColorB, _ColorA, i.screenPos.y / i.screenPos.w);
					}
				ENDCG
			}
	}

}
