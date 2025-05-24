// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "LazyUI/Sprites/Default"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

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
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            //#pragma fragment SpriteFrag
            #pragma fragment LazySpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"


#if 1 // Lazy
            #pragma multi_compile_local LAZY_FILTER_SOFT LAZY_FILTER_LINEAR LAZY_FILTER_COS LAZY_FILTER_EDGE LAZY_FILTER_POINT LAZY_FILTER_SHARP
            #pragma multi_compile_local _ LAZY_ANTI_ALPHA_ON
            #include "LazyCG.cginc"
            float4 _MainTex_TexelSize;

            fixed4 LazySampleSpriteTexture(float2 uv)
            {
                fixed4 color = LazyTex2D(_MainTex, uv, _MainTex_TexelSize);
                return color;
            }

            fixed4 LazySpriteFrag(v2f IN) : SV_Target
            {
                //fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                fixed4 c = LazySampleSpriteTexture(IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
#endif // Lazy
        ENDCG
        }
    }
}
