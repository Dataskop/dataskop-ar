Shader "Dataskop/BarFrameOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Frame Color", Color) = (1,1,1,1)
        _Cull("Cull", Float) = 2.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        _PlaneTransparency("Plane Transparency", Float) = 0.25
        _FrameTransparency("Frame Transparency", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite[_ZWrite]
        Cull[_Cull]
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                float  _Rotation;
                float  _PlaneTransparency;
                float  _FrameTransparency;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(float3(v.vertex.x, v.vertex.y, v.vertex.z));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                if(any(col.rgb == half3(0, 0, 0)))
                {
                    return col * half4(1, 1, 1, _PlaneTransparency);
                }

                return _Color * half4(1, 1, 1, _FrameTransparency);
            }
            ENDHLSL
        }
    }
}