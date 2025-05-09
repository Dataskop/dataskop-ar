Shader "Dataskop/VisDot"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Ring Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="true"
            "PreviewType"="Plane"
            "SortingLayer"="Custom/WorldSpaceCanvas"
            "Order in Layer"="0"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                half3 rgb = col.rgb;

                // linear RGB to luminance/brightness (luma)
                float brightness = dot(rgb, half3(0.222, 0.707, 0.071));
                float blendFactor = smoothstep(0.0, 0.5, 1.0 - brightness); // Adjust thresholds as needed

                //  blend based on the blendFactor
                half3 blend_rgb = lerp(half3(1, 1, 1), _Color.rgb, blendFactor);
                half4 blend_color = half4(blend_rgb, col.a);

                UNITY_APPLY_FOG(i.fogCoord, blend_color);
                return blend_color;
            }
            ENDHLSL
        }
    }
}