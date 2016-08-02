// Diffuse Shadowmapped - Vertex Lit CG Version
Shader "Custom/Unlit-Tinted"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
		_TintMask("Tint Mak", 2D) = "black" {}
		_TintColor("Tint", Color) = (0.0, 0.0, 0.0, 0.0)
		_Color1("Color 1", Color) = (1.0, 1.0, 1.0, 1.0)
		_Color2("Color 2", Color) = (1.0, 1.0, 1.0, 1.0)
		_Color3("Color 3", Color) = (1.0, 1.0, 1.0, 1.0)
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
			fixed4 	  _Color1;
			fixed4 	  _Color2;
			fixed4 	  _Color3;

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


			float3 rgb2hsv(fixed3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			fixed3 hsv2rgb(fixed3 c)
			{
				fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
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

				fixed3 tint = rgb2hsv(_Color1.rgb);

				//fixed3 r = lerp(c.rgb, ShiftColor(c.rgb, fixed3(0,0,1)) * _Color1.rgb , m.r);
				fixed3 r = lerp(c.rgb, ShiftColor(c.rgb, fixed3(tint.r * -360, tint.g, 1)), m.r);
				
	
				return fixed4(r, 1);
			}

			ENDCG
		}
	}
	Fallback "Mobile/Diffuse"
}