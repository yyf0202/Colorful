Shader "Unlit/CharactorWithPlanerShadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "PlanerShadow"

			Stencil {
				Ref 0
				Comp equal
				Pass incrWrap
				Fail Keep
				ZFail Keep
			}

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Offset -1 , 0

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
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			float4 _LightDir;
			float4 _ShadowColor;
			float _ShadowAttenuation;
			
			float3 ShadowProjPos(float4 vertPos){
				float3 shadowWorldPos;
				float3 worldPos = mul(unity_ObjectToWorld,vertPos).xyz;
				float3 lightDir = normalize(_LightDir.xyz);
				shadowWorldPos.y = min(worldPos.y,_LightDir.w);
				shadowWorldPos.xz = worldPos.xz - lightDir.xz * max(0,worldPos.y - _LightDir.w) / lightDir.y;
				return shadowWorldPos;
			}

			v2f vert (appdata v)
			{
				v2f o;
				float3 shadowWorldPos = ShadowProjPos(v.vertex);
				float3 center = float3(unity_ObjectToWorld[0].w,_LightDir.w,unity_ObjectToWorld[2].w);
				float fallOff = 1 -	saturate(distance(shadowWorldPos,center)*_ShadowAttenuation);
				o.vertex =UnityWorldToClipPos(shadowWorldPos);
				o.color = _ShadowColor;
				o.color.a *= fallOff;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
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
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
