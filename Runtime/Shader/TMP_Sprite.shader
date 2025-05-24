Shader "LazyUI/TextMeshPro/Sprite"
{
	Properties
	{
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		
		_CullMode ("Cull Mode", Float) = 0
		_ColorMask ("Color Mask", Float) = 15
		_ClipRect ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        [KeywordEnum(Soft, Linear, Cos, Edge, Point, Sharp)]Lazy_Filter("Filter", Int) = 0
        [Toggle] Lazy_Anti_Alpha("Alpha leakage suppression", Float) = 0
        [Toggle] Lazy_Snap("Snap Whole Numbers", Float) = 0
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
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull [_CullMode]
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
            Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
            #include "LazyCG.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
			
            #pragma multi_compile_local LAZY_FILTER_SOFT LAZY_FILTER_LINEAR LAZY_FILTER_COS LAZY_FILTER_EDGE LAZY_FILTER_POINT LAZY_FILTER_SHARP
            #pragma multi_compile_local _ LAZY_ANTI_ALPHA_ON
            #pragma multi_compile_local _ LAZY_SNAP_ON

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
			};
			
            sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t v)
			{
				v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
#if LAZY_SNAP_ON
                //float4 vPosition = UnityObjectToClipPos(floor(v.vertex));
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                vPosition.xy = (round(vPosition.xy * (_ScreenParams.xy * 0.5))) * ((_ScreenParams.zw - 1.0) * 2.0);
				OUT.vertex  = vPosition;
#else
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
#endif

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				
                OUT.color = v.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
#if 1//Lazy
                half4 color = (LazyTex2D(_MainTex, IN.texcoord, _MainTex_TexelSize) + _TextureSampleAdd) * IN.color;
#else
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
#endif
				
                #ifdef UNITY_UI_CLIP_RECT
					color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
					clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
}
