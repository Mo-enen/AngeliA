Shader "Angelia/Lerp"
{
	Properties
	{
		[HideInInspector] _MainTex("Texture", 2D) = "white" {}
		[HideInInspector] _UserTex("Texture", 2D) = "white" {}
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
sampler2D _UserTex;

		struct appdata
		{
			float4 vertex : POSITION;
			float4 color : Color;
			float2 uv : TEXCOORD0;

		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 color : Color;
			float4 vertex : SV_POSITION;
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
    if (i.uv.x < 1)
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        half4 rgb = i.color;
        rgb.a = col.a;
        return lerp(rgb, col, i.color.a);
    }
    else
    {
        fixed4 col = tex2D(_UserTex, i.uv);
        half4 rgb = i.color;
        rgb.a = col.a;
        return lerp(rgb, col, i.color.a);
    }
			
		}
	ENDCG
	}
	}
}

