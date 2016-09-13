// Unlit-HSV-Tinted
Shader "Custom/Unlit-HSV-Tinted"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
		_TintMask("Tint Mask", 2D) = "black" {}
		_Color1("Color 1", Color) = (1.0, 1.0, 1.0, 1.0)
		_Color2("Color 2", Color) = (1.0, 1.0, 1.0, 1.0)
		_Color3("Color 3", Color) = (1.0, 1.0, 1.0, 1.0)
		_Color4("Color 4", Color) = (1.0, 1.0, 1.0, 1.0)
		
		_Distance("Distance", Float) = 0.015
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass
		{
			Tags{ "LightMode" = "Vertex" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			half4	  _MainTex_ST;
			sampler2D _TintMask;
			half4	  _TintMask_ST;
			fixed4 	  _Color1;
			fixed4 	  _Color2;
			fixed4 	  _Color3;
			fixed4 	  _Color4;

			float _Distance;

			struct v2f
			{
				float4  position: POSITION;
				half2   uv_MainTex: TEXCOORD0;
				half2   uv_TintMask: TEXCOORD1;
			};
		
			fixed3 rgb2hsv(fixed3 c)
			{
			    fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				fixed4 p = c.g < c.b ? fixed4(c.bg, K.wz) : fixed4(c.gb, K.xy);
			    fixed4 q = c.r < p.x ? fixed4(p.xyw, c.r) : fixed4(c.r, p.yzx);

			    fixed d = q.x - min(q.w, q.y);
			    fixed e = 1.0e-10;
			    return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 hsv2rgb(fixed3 c)
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
				fixed4 hsv = tex2D(_MainTex, i.uv_MainTex);

				fixed4 mask = tex2D(_TintMask, i.uv_TintMask);
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x + _Distance, i.uv_TintMask.y + _Distance));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x + _Distance, i.uv_TintMask.y));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x, i.uv_TintMask.y + _Distance));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x - _Distance, i.uv_TintMask.y - _Distance));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x + _Distance, i.uv_TintMask.y - _Distance));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x - _Distance, i.uv_TintMask.y + _Distance));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x - _Distance, i.uv_TintMask.y));
				mask += tex2D(_TintMask, half2(i.uv_TintMask.x, i.uv_TintMask.y - _Distance));
				mask = mask / 9;




				fixed3 photoRGB = hsv2rgb(hsv.rgb);

				fixed3 result1 = lerp(photoRGB, hsv2rgb(_Color1.rgb) * hsv.b, mask.r); // Replace color with mask

				fixed3 result2 = lerp(photoRGB, hsv2rgb(_Color2.rgb) * hsv.b, mask.g);

				fixed3 result3 = lerp(photoRGB, hsv2rgb(_Color3.rgb) * hsv.b, mask.b);

				fixed3 result4 = lerp(photoRGB, hsv2rgb(_Color4.rgb) * hsv.b, mask.a);

				fixed3 result = lerp(result1, result2, mask.g);
				result = lerp(result , result3, mask.b);
				result = lerp(result, result4, mask.a);
	
				return fixed4(result, 1);
			}

			ENDCG
		}
	}
	Fallback "Mobile/Diffuse"
}