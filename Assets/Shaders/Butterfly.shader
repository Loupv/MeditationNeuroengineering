Shader "Custom/Butterfly"
    
{
    Properties
    {
        [NoScaleOffset]        _Albedo("Albedo (RGB), Alpha (A)", 2D) = "white" {}
        [NoScaleOffset]        _Metallic("Metallic (R), Occlusion (G), Emission (B), Smoothness (A)", 2D) = "black" {}
        [NoScaleOffset]        _Normal("Normal (RGB)", 2D) = "bump" {}
        [NoScaleOffset]        _DispTex("Displacement Texture", 2D) = "white" {}
        //_Color ("Color (RGBA)", Color) = (1, 1, 1, 1) // add _Color property
        _Amount("Displacement Amount", Range(0,3)) = 0.5
        _Speed("Speed", Range(0,3)) = 0.1
        _Color("Color", Color) = (1,1,1,1)
    }
 
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
		Lighting On
        Cull Off
        LOD 100

        CGINCLUDE
        #define _GLOSSYENV 1
        ENDCG
 
        CGPROGRAM
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"
        #pragma surface surf Standard vertex:vert fullforwardshadows alpha:fade
        #pragma exclude_renderers gles
        #pragma target 3.0
 
        struct Input
        {
            float2 uv_Albedo;
        };
 
        sampler2D _Albedo;
        sampler2D _Normal;
        sampler2D _Metallic;

        float4 _Color;

        float _Amount;
        float _Speed;
        sampler2D _DispTex;
 
        void vert(inout appdata_full v)
        {
            v.vertex.xyz += v.normal * tex2Dlod(_DispTex, float4(v.texcoord.xy, 0, 0)).r * _Amount * sin(_Time * 30 * _Speed);
        }

        /*fixed4 frag (v2f i) : SV_Target
        {
            fixed4 col = tex2D(_Albedo, i.texcoord) * _Color; // multiply by _Color
            return col;
        }*/
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 albedo = tex2D(_Albedo, IN.uv_Albedo) * _Color;
            
            fixed4 metallic = tex2D(_Metallic, IN.uv_Albedo);
            //fixed3 normal = UnpackScaleNormal(tex2D(_Normal, IN.uv_Albedo), 1);
 
            o.Albedo = albedo.rgb;
            o.Alpha = albedo.a;
            //o.Normal = normal;
            o.Smoothness = metallic.a;
            o.Occlusion = metallic.g;
            o.Emission = metallic.b;
            o.Metallic = metallic.r;
        }

        ENDCG
    }
 
        FallBack "Standard"
}

