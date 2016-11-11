Shader "BilateralFilter"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			#define SIGMA 10.0
			#define BSIGMA 0.1
			#define MSIZE 20

			fixed normpdf(in float x, in float sigma)
			{
				return 0.39894*exp(-0.5*x*x/(sigma*sigma))/sigma;
			}

			fixed normpdf3(in float3 v, in float sigma)
			{
				return 0.39894*exp(-0.5*dot(v,v)/(sigma*sigma))/sigma;
			}
		
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
	
			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 c = tex2D(_MainTex, i.uv);

				const int kSize = (MSIZE-1)/2;
				fixed kernel[MSIZE];
				fixed3 final_colour = 0; 

				fixed Z = 0.0;

				for (int j = 0; j <= kSize; ++j)
				{
					kernel[kSize+j] = kernel[kSize-j] = normpdf(float(j), SIGMA);
				}

				float2 uv = i.uv.xy;

				fixed3 cc;
				fixed factor;
				fixed bZ = 1.0 / normpdf(0.0, BSIGMA);
				//read out the texels
				for (int i=-kSize; i <= kSize; ++i)
				{
					for (int j=-kSize; j <= kSize; ++j)
					{
						cc = tex2D(_MainTex, uv.xy + float2(i / _ScreenParams.x, j / _ScreenParams.y)).rgb;
						factor = normpdf3(cc-c, BSIGMA)*bZ*kernel[kSize+j]*kernel[kSize+i];
						Z += factor;
						final_colour += factor*cc;
					}
				}

				return fixed4(final_colour / Z, 1.0);;
			}
			ENDCG
		}
	}
}