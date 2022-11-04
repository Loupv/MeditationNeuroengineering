
Shader "Custom/Cloud/ParaboloidFragWorldSizeShader"
{
	
	

	/*
	This shader renders the given vertices as circles with the given color.
	The point size is the radius of the circle given in WORLD COORDINATES
	Implemented using geometry shader.
	Interpolation is done by creating screen facing paraboloids in the fragment shader!
	*/
	Properties{
		_PointSize("Point Size", Float) = 0.0013
		[Toggle] _Circles("Circles", Int) = 1
		[Toggle] _OBBFiltering("OBBFiltering", Int) = 0
		[Toggle] _Cones("Cones", Int) = 0
		_Tint("Tint", Color) = (0.5, 0.5, 0.5, 1)

		_offset ("Offset",Vector) = (1.0,1.0,1.0)
        _cosFreq ("Offset Frequency",Vector) = (1.0,1.0,1.0)
		
		_GlowingSpherePosition("GlowingSpherePosition", Vector) = (0,0,0)
		_GlowingSphereRadius("GlowingSphereRadius", float) = 2

		_ColorGain("ColorGain", float) = 1
        /*_octaves ("Octaves",Int) = 7
        _lacunarity("Lacunarity", Range( 1.0 , 5.0)) = 2
        _gain("Gain", Range( 0.0 , 1.0)) = 0.5
        _value("Value", Range( -2.0 , 2.0)) = 0.0
        _amplitude("Amplitude", Range( 0.0 , 5.0)) = 1.5
        _frequency("Frequency", Range( 0.0 , 6.0)) = 2.0
        _power("Power", Range( 0.1 , 5.0)) = 1.0
        _scale("Scale", Float) = 1.0
        _color ("Color", Color) = (1.0,1.0,1.0,1.0)      
        [Toggle] _monochromatic("Monochromatic", Float) = 0
        _range("Monochromatic Range", Range( 0.0 , 1.0)) = 0.5   */ 
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		ZWrite On
		LOD 200
		Cull off

		Pass
		{
	
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			const float PI = float(3.14159265359);

			struct VertexInput
			{
				float4 position : POSITION;
				float4 color : COLOR;
			};

			struct VertexMiddle {
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float4 R : NORMAL0;
				float4 U : NORMAL1;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float4 viewposition : TEXCOORD1;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct FragmentOutput {
				float4 color : SV_TARGET;
				float depth : SV_DEPTH;
			};

			float _PointSize;
			int _Circles;
			int _Cones;
			float4 _Tint;
			int _OBBFiltering;

			float4 _ObbsPos[10];
			float4 _ObbsSize[10];
			float4x4 _ObbsOrientation[10];

			float3 _offset,_cosFreq;
			float _ColorGain;

			float3 _GlowingSpherePosition;
			float _GlowingSphereRadius;

			// *********************** External noise functions 
			
			#define NOISE_SIMPLEX_1_DIV_289 0.00346020761245674740484429065744f
 
			float mod289(float x) {
				return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
			}
 
			float2 mod289(float2 x) {
				return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
			}
 
			float3 mod289(float3 x) {
				return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
			}
 
			float4 mod289(float4 x) {
				return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
			}
 
 
			// ( x*34.0 + 1.0 )*x =
			// x*x*34.0 + x
			float permute(float x) {
				return mod289(
					x*x*34.0 + x
				);
			}
 
			float3 permute(float3 x) {
				return mod289(
					x*x*34.0 + x
				);
			}
 
			float4 permute(float4 x) {
				return mod289(
					x*x*34.0 + x
				);
			}
 
 
 
			float taylorInvSqrt(float r) {
				return 1.79284291400159 - 0.85373472095314 * r;
			}
 
			float4 taylorInvSqrt(float4 r) {
				return 1.79284291400159 - 0.85373472095314 * r;
			}
			
			float remap(float value, float low1, float high1, float low2, float high2) {
				if (value > high1) return high2;
				else if (value < low1) return low2;
				else return (value - low1) * (high2 - low2) / (high1 - low1) + low2;
			}
 
 
			float4 grad4(float j, float4 ip)
			{
				const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
				float4 p, s;
				p.xyz = floor( frac(j * ip.xyz) * 7.0) * ip.z - 1.0;
				p.w = 1.5 - dot( abs(p.xyz), ones.xyz );
 
				// GLSL: lessThan(x, y) = x < y
				// HLSL: 1 - step(y, x) = x < y
				s = float4(
					1 - step(0.0, p)
				);
 
				// Optimization hint Dolkar
				// p.xyz = p.xyz + (s.xyz * 2 - 1) * s.www;
				p.xyz -= sign(p.xyz) * (p.w < 0);
 
				return p;
			}


			float snoise(float3 v)
			{
				const float2 C = float2(
					0.166666666666666667, // 1/6
					0.333333333333333333 // 1/3
				);
				const float4 D = float4(0.0, 0.5, 1.0, 2.0);
			// First corner
				float3 i = floor(v + dot(v, C.yyy));
				float3 x0 = v - i + dot(i, C.xxx);
			// Other corners
				float3 g = step(x0.yzx, x0.xyz);
				float3 l = 1 - g;
				float3 i1 = min(g.xyz, l.zxy);
				float3 i2 = max(g.xyz, l.zxy);
				float3 x1 = x0 - i1 + C.xxx;
				float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
				float3 x3 = x0 - D.yyy; // -1.0+3.0*C.x = -0.5 = -D.y
			// Permutations
				i = mod289(i);
				float4 p = permute(
					permute(
						permute(
								i.z + float4(0.0, i1.z, i2.z, 1.0)
						) + i.y + float4(0.0, i1.y, i2.y, 1.0)
					) + i.x + float4(0.0, i1.x, i2.x, 1.0)
				);
			// Gradients: 7x7 points over a square, mapped onto an octahedron.
			// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
				float n_ = 0.142857142857; // 1/7
				float3 ns = n_ * D.wyz - D.xzx;
				float4 j = p - 49.0 * floor(p * ns.z * ns.z); // mod(p,7*7)
				float4 x_ = floor(j * ns.z);
				float4 y_ = floor(j - 7.0 * x_); // mod(j,N)
				float4 x = x_ * ns.x + ns.yyyy;
				float4 y = y_ * ns.x + ns.yyyy;
				float4 h = 1.0 - abs(x) - abs(y);
				float4 b0 = float4(x.xy, y.xy);
				float4 b1 = float4(x.zw, y.zw);
				//float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
				//float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
				float4 s0 = floor(b0) * 2.0 + 1.0;
				float4 s1 = floor(b1) * 2.0 + 1.0;
				float4 sh = -step(h, float4(0, 0, 0, 0));
				float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
				float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
				float3 p0 = float3(a0.xy, h.x);
				float3 p1 = float3(a0.zw, h.y);
				float3 p2 = float3(a1.xy, h.z);
				float3 p3 = float3(a1.zw, h.w);
			//Normalise gradients
				float4 norm = rsqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
				p0 *= norm.x;
				p1 *= norm.y;
				p2 *= norm.z;
				p3 *= norm.w;
			// Mix final noise value
				float4 m = max(0.5 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
				m = m * m;
				return 105.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
			}


			// *********************** Florian 

			float4 valid_vertex(float4 p) {

				for (int idOBB = 0; idOBB < 10; ++idOBB) {

					if (_ObbsSize[idOBB].w == 1.f) {

						float3 dir = p.xyz - _ObbsPos[idOBB];
						bool inside = true;

						for (int idV = 0; idV < 3; ++idV) {

							if (inside) {

								float d = dot(dir, _ObbsOrientation[idOBB][idV].xyz);
								if (d > _ObbsSize[idOBB][idV]) {
									inside = false;
								}
								if (d < -_ObbsSize[idOBB][idV]) {
									inside = false;
								}
							}
						}

						if (inside) {
							return float4(p.x , p.y, p.z, 1.0);
						}
					}
				}

				return float4(p.x, p.y, p.z, 0.0);
			}


			VertexMiddle vert(VertexInput v) {

				VertexMiddle o;
				if (_OBBFiltering == 1) {
					o.position = float4(v.position.xyz, valid_vertex(mul(unity_ObjectToWorld, v.position)).w);
				}
				else {
					o.position = float4(v.position.xyz, 1);
				}
				

				float4 col = v.color;
				float3 cc =  LinearToGammaSpace(_Tint.rgb) * float3(col.r,col.g,col.b);
				cc = GammaToLinearSpace(cc);		
				
				
				float _p = snoise( float3(o.position.x * _cosFreq.x + cos(_Time.x), o.position.y * _cosFreq.y + sin(_Time.x), o.position.z * _cosFreq.z));

				// Color

				float dist = distance(v.position, _GlowingSpherePosition);


				float distGain = (pow(1 / remap(dist, 0, _GlowingSphereRadius, 0.01, 1), (abs(cos(_Time.y / 1.5)) * 2 + 1)));
				cc *= (_ColorGain+abs(_p)) * distGain;

				
				o.color = float4(cc, 1);


				// Position noise
				float3 view = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float3 upvec = normalize(UNITY_MATRIX_IT_MV[1].xyz);
				float3 R = normalize(cross(view, upvec));

				float pointSizeMultiplier = 0.75+1/(4*exp(1/abs(distGain)));

				o.U = float4(upvec * _PointSize * pointSizeMultiplier, 0);
				o.R = -float4(R * _PointSize * pointSizeMultiplier, 0);
				
				o.position.x += _p* _offset.x * cos(_Time.y/10 * _cosFreq.x+_p);
				o.position.y += _p* _offset.y * cos(_Time.y/10 * _cosFreq.y+_p);
				o.position.z += _p* _offset.z * cos(_Time.y/10 * _cosFreq.z+_p);

				return o;
			}

			[maxvertexcount(4)]
			void geom(point VertexMiddle input[1], inout TriangleStream<VertexOutput> outputStream) {

				if (input[0].position.w < 0.5) {
					return;
				}

				VertexOutput out1;
				out1.position = input[0].position;
				out1.color = input[0].color;
				out1.uv = float2(-1.0f, 1.0f);
				out1.position += (-input[0].R + input[0].U);
				out1.viewposition = mul(UNITY_MATRIX_MV, out1.position);
				out1.position = UnityObjectToClipPos(out1.position);
				VertexOutput out2;
				out2.position = input[0].position;
				out2.color = input[0].color;
				out2.uv = float2(1.0f, 1.0f);
				out2.position += (input[0].R + input[0].U);
				out2.viewposition = mul(UNITY_MATRIX_MV, out2.position);
				out2.position = UnityObjectToClipPos(out2.position);
				VertexOutput out3;
				out3.position = input[0].position;
				out3.color = input[0].color;
				out3.uv = float2(1.0f, -1.0f);
				out3.position += (input[0].R - input[0].U);
				out3.viewposition = mul(UNITY_MATRIX_MV, out3.position);
				out3.position = UnityObjectToClipPos(out3.position);
				VertexOutput out4;
				out4.position = input[0].position;
				out4.color = input[0].color;
				out4.uv = float2(-1.0f, -1.0f);
				out4.position += (-input[0].R - input[0].U);
				out4.viewposition = mul(UNITY_MATRIX_MV, out4.position);
				out4.position = UnityObjectToClipPos(out4.position);
				outputStream.Append(out1);
				outputStream.Append(out2);
				outputStream.Append(out4);
				outputStream.Append(out3);
			}

			FragmentOutput frag(VertexOutput o) {
				FragmentOutput fragout;
				float uvlen = o.uv.x*o.uv.x + o.uv.y*o.uv.y;
				if (_Circles >= 0.5 && uvlen > 1) {
					discard;
				}
				if (_Cones < 0.5) {
					o.viewposition.z += (1 - uvlen) * _PointSize;
				}
				else {
					o.viewposition.z += (1 - sqrt(uvlen)) * _PointSize;
				}
				float4 pos = mul(UNITY_MATRIX_P, o.viewposition);
				pos /= pos.w;
				fragout.depth = pos.z;
				fragout.color = o.color;
				return fragout;
			}


			

			ENDCG
		}
	}
}
