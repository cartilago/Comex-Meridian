// Diffuse Shadowmapped - Vertex Lit CG Version
Shader "Custom/Unlit-Tinted"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
		_TintMask("Tint Mak", 2D) = "black" {}
		_TintColor("Tint", Color) = (0.0, 0.0, 0.0, 0.0)
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
			sampler2D _TintMask;
			half4	  _TintMask_ST;
			fixed4 	  _TintColor;

			struct v2f
			{
				float4  position: POSITION;
				half2   uv_MainTex: TEXCOORD0;
				half2   uv_TintMask: TEXCOORD1;
			};

			// HSV shifting function
			fixed3 ShiftColor(fixed3 RGB, fixed3 shift)
			{
				fixed3 result = 1;
				fixed VSU = shift.z * shift.y * cos(shift.x * 3.14159265 / 180);
				fixed VSW = shift.z * shift.y * sin(shift.x * 3.14159265 / 180);

				result.x = (.299*shift.z + .701*VSU + .168*VSW)*RGB.x
					+ (.587*shift.z - .587*VSU + .330*VSW)*RGB.y
					+ (.114*shift.z - .114*VSU - .497*VSW)*RGB.z;

				result.y = (.299*shift.z - .299*VSU - .328*VSW)*RGB.x
					+ (.587*shift.z + .413*VSU + .035*VSW)*RGB.y
					+ (.114*shift.z - .114*VSU + .292*VSW)*RGB.z;

				result.z = (.299*shift.z - .3*VSU + 1.25*VSW)*RGB.x
					+ (.587*shift.z - .588*VSU - 1.05*VSW)*RGB.y
					+ (.114*shift.z + .886*VSU - .203*VSW)*RGB.z;

				return (result);
			}

			v2f vert(appdata_base v)
			{
				v2f o;

				UNITY_INITIALIZE_OUTPUT(v2f, o);

				// Transform vertices & texture coords
				o.position = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				o.uv_TintMask = TRANSFORM_TEX(v.texcoord.xy, _TintMask);

				return o;
			}

			fixed4 frag(v2f i) :COLOR
			{
				fixed4 c = tex2D(_MainTex, i.uv_MainTex);
				fixed4 m = tex2D(_TintMask, i.uv_TintMask);

				fixed3 r = lerp(c.rgb, ShiftColor(c.rgb, fixed3(100,1,1)), m.r);
	
				return fixed4(r, 1);
			}

			ENDCG
		}
	}
	Fallback "Mobile/Diffuse"
}