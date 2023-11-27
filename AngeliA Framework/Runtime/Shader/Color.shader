Shader "Angelia/Color"
{
	Properties
	{
		[HideInInspector] _MainTex("Texture", 2D) = "white" {}
	}

		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;


			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed4 color : Color;
				fixed2 uv : TEXCOORD0;
			};


			struct v2f
			{
				fixed4 vertex : SV_POSITION;
				fixed4 color : Color;
				fixed2 uv : TEXCOORD0;
			};


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
                fixed4 tColor = i.color;
                tColor.a *= tex2D(_MainTex, i.uv).a;
                return tColor;
}
		ENDCG
		}
	}
}

