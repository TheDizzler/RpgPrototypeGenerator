Shader "AtomosZ/NoSwapShader"
{
	Properties
	{
		
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HideInInspector] _SwapTex("Color Data", 2D) = "transparent" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Offset("RelativeZ", Float) = -1
			
	}
		SubShader
		{
			cull off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				AlphaToMask On
				Offset -1, [_Offset]
				CGPROGRAM
				#pragma vertex vertexFunc
				#pragma fragment fragmentFunc


				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};


				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 uv : TEXCOORD0;
					float4 vertex : TEXCOORD2;
				};


				sampler2D _MainTex;


				v2f vertexFunc(appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord;
					o.vertex = v.vertex;
					return o;
				}


				fixed4 fragmentFunc(v2f i) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, i.uv);

					return c;
				}

			ENDCG
		}
		}
}
