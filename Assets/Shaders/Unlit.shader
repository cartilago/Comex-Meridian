Shader "Custom/Unlit"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass
		{
			Tags{ "LightMode" = "Vertex" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4	  _MainTex_ST;

			struct v2f
			{
				float4  position: POSITION;
				half2   uv_MainTex: TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				UNITY_INITIALIZE_OUTPUT(v2f, o);

				// Transform vertices & texture coords
				o.position = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) :COLOR
			{
				return tex2D(_MainTex, i.uv_MainTex);
			}

			ENDCG
		}
	}
	Fallback "Mobile/Diffuse"
}