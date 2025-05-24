// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader"LazyUI/Sprites/Mask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _Cutoff ("Mask alpha cutoff", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,0.2)
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [KeywordEnum(Soft, Linear, Cos, Edge, Point, Sharp)]Lazy_Filter("Filter", Int) = 0
        [Toggle] Lazy_Anti_Alpha("Alpha leakage suppression", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend Off
        ColorMask 0

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            #include "LazyCG.cginc"

            // alpha below which a mask should discard a pixel, thereby preventing the stencil buffer from being marked with the Mask's presence
            fixed _Cutoff;

            struct appdata_masking
            {
                float4 vertex : POSITION;
                half2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f_masking
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f_masking vert(appdata_masking IN)
            {
                v2f_masking OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.pos = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;

                #ifdef PIXELSNAP_ON
                OUT.pos = UnityPixelSnap (OUT.pos);
                #endif

                return OUT;
            }

#if 1 // Lazy
            #pragma multi_compile_local LAZY_FILTER_LINEAR LAZY_FILTER_COS LAZY_FILTER_EDGE LAZY_FILTER_POINT LAZY_FILTER_SHARP
            #pragma multi_compile_local _ LAZY_ANTI_ALPHA_ON
            #include "LazyCG.cginc"
            float4 _MainTex_TexelSize;

            fixed4 LazySampleSpriteTexture(float2 uv)
            {
                fixed4 color = LazyTex2D(_MainTex, uv, _MainTex_TexelSize);
                return color;
            }
#endif

            fixed4 frag(v2f_masking IN) : SV_Target
            {
                //fixed4 c = SampleSpriteTexture(IN.uv);
                fixed4 c = LazySampleSpriteTexture(IN.uv);
                // for masks: discard pixel if alpha falls below MaskingCutoff
                clip (c.a - _Cutoff);
                return _Color;
            }
        ENDCG
        }
    }
}
