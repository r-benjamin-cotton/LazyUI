// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader"LazyUI/Unlit/Texture" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    [KeywordEnum(Soft, Linear, Cos, Edge, Point, Sharp)]Lazy_Filter("Filter", Int) = 0
    [Toggle] Lazy_Anti_Alpha("Alpha leakage suppression", Float) = 0
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
#if 1 // Lazy
            #pragma multi_compile_local LAZY_FILTER_SOFT LAZY_FILTER_LINEAR LAZY_FILTER_COS LAZY_FILTER_EDGE LAZY_FILTER_POINT LAZY_FILTER_SHARP
            #pragma multi_compile_local _ LAZY_ANTI_ALPHA_ON
            #include "../LazyCG.cginc"
            float4 _MainTex_TexelSize;
#endif

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.texcoord);
                fixed4 col = LazyTex2D(_MainTex, i.texcoord, _MainTex_TexelSize);
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
}

}
