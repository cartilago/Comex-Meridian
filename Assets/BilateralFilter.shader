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
			#pragma target 3.5 
			
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
			#define MSIZE 15

			float normpdf(in float x, in float sigma)
			{
				return 0.39894*exp(-0.5*x*x/(sigma*sigma))/sigma;
			}

			float normpdf3(in float3 v, in float sigma)
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
	
			float4 frag (v2f i) : SV_Target
			{
				float3 c = tex2D(_MainTex, i.uv); //, float2(0.0, 1.0) - (i.uv.xy / _ScreenParams.xy)).rgb;

				const int kSize = (MSIZE-1)/2;
				float kernel[MSIZE];
				float3 final_colour = 0; 

				float Z = 0.0;

				for (int j = 0; j <= kSize; ++j)
				{
					kernel[kSize+j] = kernel[kSize-j] = normpdf(float(j), SIGMA);
				}

				float2 uv = i.uv.xy;

				float3 cc;
				float factor;
				float bZ = 1.0 / normpdf(0.0, BSIGMA);
				//read out the texels
				for (int i=-kSize; i <= kSize; ++i)
				{
					for (int j=-kSize; j <= kSize; ++j)
					{
						//cc = tex2D(_MainTex, float2(0.0, 1.0) - (uv.xy+float2(float(i),float(j))) /  _ScreenParams.xy).rgb;
						cc = tex2D(_MainTex, uv.xy + float2(i / _ScreenParams.x, j / _ScreenParams.y)).rgb;
						factor = normpdf3(cc-c, BSIGMA)*bZ*kernel[kSize+j]*kernel[kSize+i];
						Z += factor;
						final_colour += factor*cc;
					}
				}

				return float4(final_colour / Z, 1.0);;
			}
			ENDCG
		}
	}
}


/*

#pragma parameter RAD   "BILATERAL Radius"        2.00 0.00 12.0 0.25
#pragma parameter CLR   "BILATERAL Color Thresh"  0.15 0.01  1.0 0.01
#pragma parameter CWGHT "BILATERAL Central Wght"  0.25 0.00  2.0 0.05

#ifdef PARAMETER_UNIFORM
	uniform float RAD, CLR, CWGHT;
#else
	#define RAD   2.00
	#define CLR   0.15
	#define CWGHT 0.25
#endif


#define TEX(dx,dy) tex2D(decal, VAR.texCoord+float2((dx),(dy))*VAR.t1)

static float4 unit4  = float4(1.0);

static int   steps = ceil(RAD);
static float clr   = -CLR * CLR;
static float sigma = RAD * RAD / 2.0;
static float cwght = 1.0 + CWGHT * max(1.0, 2.87029746*sigma + 0.43165242*RAD - 0.25219746);

static float domain[13] = {1.0, exp( -1.0/sigma), exp( -4.0/sigma), exp( -9.0/sigma), exp( -16.0/sigma), exp( -25.0/sigma), exp( -36.0/sigma),
				exp(-49.0/sigma), exp(-64.0/sigma), exp(-81.0/sigma), exp(-100.0/sigma), exp(-121.0/sigma), exp(-144.0/sigma)};

float dist2(float3 pt1, float3 pt2)
{
	float3 v = pt1 - pt2;
	return dot(v,v);
}

float4 weight(int i, int j, float3 org, float4x3 A)
{
	return domain[i] * domain[j] * exp(float4(dist2(org,A[0]), dist2(org,A[1]), dist2(org,A[2]), dist2(org,A[3]))/clr);
}


struct input
{
	float2 video_size;
	float2 texture_size;
	float2 output_size;
};

struct out_vertex {
	float4 position : POSITION;
	float4 color    : COLOR;
	float2 texCoord : TEXCOORD0;
	float2 t1;
};

/*    VERTEX_SHADER    */
out_vertex main_vertex
(
	float4 position	: POSITION,
	float4 color	: COLOR,
	float2 texCoord : TEXCOORD0,

   	uniform float4x4 modelViewProj,
	uniform input IN
)
{
	out_vertex OUT;

	OUT.position = mul(modelViewProj, position);
	OUT.color = color;

	OUT.texCoord = texCoord;
	OUT.t1       = 1.0/IN.texture_size;

	return OUT;
}


/*    FRAGMENT SHADER    */
float3 main_fragment(in out_vertex VAR, uniform sampler2D decal : TEXUNIT0, uniform input IN) : COLOR
{
	float4x3 A, B;
	float4 wghtA, wghtB;
	float3 org = TEX(0,0).rgb, result = cwght*org;
	float  norm = cwght;
	

	for(int x=1; x<=steps; x++){
		
		A = float4x3(TEX( x, 0).rgb, TEX(-x, 0).rgb, TEX( 0, x).rgb, TEX( 0,-x).rgb);
		B = float4x3(TEX( x, x).rgb, TEX( x,-x).rgb, TEX(-x, x).rgb, TEX(-x,-x).rgb);

		wghtA = weight(x, 0, org, A); wghtB = weight(x, x, org, B);	

		result += mul(wghtA, A)     + mul(wghtB, B);
		norm   += dot(wghtA, unit4) + dot(wghtB, unit4);
		
		for(int y=1; y<x; y++){
					
			A = float4x3(TEX( x, y).rgb, TEX( x,-y).rgb, TEX(-x, y).rgb, TEX(-x,-y).rgb);
			B = float4x3(TEX( y, x).rgb, TEX( y,-x).rgb, TEX(-y, x).rgb, TEX(-y,-x).rgb);

			wghtA = weight(x, y, org, A); wghtB = weight(y, x, org, B);	

			result += mul(wghtA, A)     + mul(wghtB, B);
			norm   += dot(wghtA, unit4) + dot(wghtB, unit4);
		}
	}

	return result/norm;
}
*/