// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Effect/Outline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Outline_Color("Outline_Color", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        //blur Downsample pass
        Pass {
            ZWrite off Fog {mode off}Cull off ZTest Always Blend Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
                
            sampler2D _MainTex; 
            uniform float4 _MainTex_TexelSize; 
            float outterLineSize; 
            struct v2f {
                float4 pos : POSITION; 
                float2 uv : TEXCOORD0; 
            }; 
            
            v2f vert(appdata_img v) {
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.uv.xy = v.texcoord.xy; 
                
                return o; 
            }
            
            half4 frag(v2f i) : COLOR {
                
                float2 texelSize = _MainTex_TexelSize.xy; 
                float2 uv = i.uv; 
                half4 A = tex2D(_MainTex, (uv + texelSize * float2( - 1.0, - 1.0))); 
                half4 B = tex2D(_MainTex, (uv + texelSize * float2(0.0, - 1.0))); 
                half4 C = tex2D(_MainTex, (uv + texelSize * float2(1.0, - 1.0))); 
                half4 D = tex2D(_MainTex, (uv + texelSize * float2( - 0.5, - 0.5))); 
                half4 E = tex2D(_MainTex, (uv + texelSize * float2(0.5, - 0.5))); 
                half4 F = tex2D(_MainTex, (uv + texelSize * float2( - 1.0, 0.0))); 
                half4 G = tex2D(_MainTex, (uv)); 
                half4 H = tex2D(_MainTex, (uv + texelSize * float2(1.0, 0.0))); 
                half4 I = tex2D(_MainTex, (uv + texelSize * float2( - 0.5, 0.5))); 
                half4 J = tex2D(_MainTex, (uv + texelSize * float2(0.5, 0.5))); 
                half4 K = tex2D(_MainTex, (uv + texelSize * float2( - 1.0, 1.0))); 
                half4 L = tex2D(_MainTex, (uv + texelSize * float2(0.0, 1.0))); 
                half4 M = tex2D(_MainTex, (uv + texelSize * float2(1.0, 1.0))); 
                
                half2 div = (1.0 / 4.0) * half2(0.5, 0.125); 
                
                half4 o = (D + E + I + J) * div.x; 
                o += (A + B + G + F) * div.y; 
                o += (B + C + H + G) * div.y; 
                o += (F + G + L + K) * div.y; 
                o += (G + H + M + L) * div.y; 
                
                return o; 
            }
            
            ENDCG
        }

        //blur Upsample pass
        Pass {
            ZWrite off Fog {mode off}Cull off ZTest Always Blend Off
            CGPROGRAM
            #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                
            sampler2D _MainTex; 
            uniform float4 _MainTex_TexelSize; 
            float outterLineSize; 
            
            struct v2f {
                float4 pos : POSITION; 
                float2 uv : TEXCOORD0; 
            }; 
            
            v2f vert(appdata_img v) {
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.uv.xy = v.texcoord.xy; 
                return o; 
            }
            
            half4 frag(v2f i) : COLOR {
                float2 texelSize = _MainTex_TexelSize.xy; 
                float2 uv = i.uv; 
                float4 d = texelSize.xyxy * float4(1.0, 1.0, - 1.0, 0.0) * outterLineSize; 
                
                half4 s; 
                s = tex2D(_MainTex, (uv - d.xy)); 
                s += tex2D(_MainTex,  (uv - d.wy)) * 2.0; 
                s += tex2D(_MainTex,  (uv - d.zy)); 
                
                s += tex2D(_MainTex,  (uv + d.zw)) * 2.0; 
                s += tex2D(_MainTex, (uv)) * 4.0; 
                s += tex2D(_MainTex,  (uv + d.xw)) * 2.0; 
                
                s += tex2D(_MainTex, (uv + d.zy)); 
                s += tex2D(_MainTex, (uv + d.wy)) * 2.0; 
                s += tex2D(_MainTex, (uv + d.xy)); 
                
                return s * (1.0 / 16.0); 

            }
            
            ENDCG
        }
        
        //Blur Pass
        Pass {
            ZWrite off Fog {mode off}Cull off ZTest Always Blend Off
            CGPROGRAM
            #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                
            sampler2D _MainTex; 
            uniform float4 _MainTex_TexelSize; 
            float4 offsets; 
            struct v2f {
                float4 pos : POSITION; 
                float2 uv : TEXCOORD0; 
                
                float4 uv01 : TEXCOORD1; 
                float4 uv23 : TEXCOORD2; 
                float4 uv45 : TEXCOORD3; 
            }; 
            v2f vert(appdata_img v) {
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.uv.xy = v.texcoord.xy; 
                
                o.uv01 = o.uv.xyxy + offsets.xyxy * float4(1, 1, - 1, - 1); 
                o.uv23 = o.uv.xyxy + offsets.xyxy * float4(1, 1, - 1, - 1) * 1.5; 
                o.uv45 = o.uv.xyxy + offsets.xyxy * float4(1, 1, - 1, - 1) * 2; 
                return o; 
            }
            half4 frag(v2f i) : COLOR {
                half4 color = float4(0, 0, 0, 0); 
                
                color += 0.4 * tex2D(_MainTex, i.uv); 
                color += 0.2 * tex2D(_MainTex, i.uv01.xy); 
                color += 0.2 * tex2D(_MainTex, i.uv01.zw); 
                color += 0.05 * tex2D(_MainTex, i.uv23.xy); 
                color += 0.05 * tex2D(_MainTex, i.uv23.zw); 
                color += 0.05 * tex2D(_MainTex, i.uv45.xy); 
                color += 0.05 * tex2D(_MainTex, i.uv45.zw); 
                return color; 
            }
            ENDCG
        }
        
        //Cut off Pass
        Pass {
            ZWrite off Fog {mode off}Cull off ZTest Always
            BlendOP RevSub
            Blend One One
            CGPROGRAM
            #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #pragma fragmentoption ARB_precision_hint_fastest
                
            sampler2D _MainTex; 
            struct v2f {
                float4 pos : SV_POSITION; 
                float4 uv : TEXCOORD0; 
            }; 
            
            v2f vert(appdata_base v)
            {
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.uv = v.texcoord; 
                return o; 
            }
            
            fixed4 frag(v2f i) : COLOR
            {
                fixed4 c = tex2D(_MainTex, i.uv); 
                return c; 
            }
            ENDCG
        }
        
        //Blit Pass
        Pass {
            ZWrite off Fog {mode off}Cull off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #pragma fragmentoption ARB_precision_hint_fastest
                
            sampler2D g_BlurSilhouette; 
            sampler2D _MainTex; 
            uniform float4 _MainTex_TexelSize; 
            struct v2f {
                float4 pos : SV_POSITION; 
                float4 uv : TEXCOORD0; 
                float4 uv2 : TEXCOORD1; 
            }; 
            
            v2f vert(appdata_base v)
            {
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.uv = v.texcoord; 
                o.uv2 = v.texcoord; 
                #if UNITY_UV_STARTS_AT_TOP
                    //if (_MainTex_TexelSize.y < 0)
                {
                    o.uv.y = 1 - o.uv.y; 
                    o.uv2.y = 1 - o.uv.y; 
                }
                #endif
                    return o; 
            }
            
            fixed4 frag(v2f i) : COLOR
            {
                fixed4 c = tex2D(_MainTex, i.uv); 
                fixed4 blur = tex2D(g_BlurSilhouette, i.uv2); 
                return fixed4(blur.rgb * blur.a + c.rgb * (1 - blur.a), 1); 
            }
            ENDCG
        }
    }
}
