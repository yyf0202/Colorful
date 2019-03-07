Shader "Sand/Sky"
{
	Properties
	{
		_ColorGround( "Ground Color" , color) = (1,1,1,1)
		_ColorSky( "Sky Color" , color ) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "RenderQueue" = "Transparent"}

		LOD 100
		cull front


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float3 worldPos :TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _ColorGround;
			float4 _ColorSky;

		
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld ,v.vertex).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = lerp( _ColorGround , _ColorSky , saturate( i.worldPos.y / 50 ));
				return col;
			}
			ENDCG
		}
	}
	FallBack "Transparent"
}
