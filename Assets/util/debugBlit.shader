Shader "Hidden/debugBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; 
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

			// 0 normal, 1 waveform, 2 spectrum
			float _RenderType; 

            fixed4 frag (v2f i) : SV_Target
            {

				i.uv.x = 1.0 - i.uv.x;
				fixed4 col = (float4)0.0;

				//spectrum
				if ( _RenderType > 1.5)
				{
					i.uv.x = pow(i.uv.x, 2);

					float val = tex2D(_MainTex, float2(i.uv.x, 0.5)).x  * 3;
					col = lerp(0, sqrt(val), smoothstep(0.05, -0.05, i.uv.y - val));

				//	col = tex2D(_MainTex, float2(i.uv.x, 0.5));
				   //col = pow(col, 0.5);
				}
				else if ( _RenderType > 0.5)
				{
                   float val = tex2D(_MainTex, float2(i.uv.x, 0.5)).x;
					col = (float4) lerp(0, pow(val, 0.5), smoothstep(0.05, -0.05, i.uv.y - val));

				}
				else // normal
				{
					float val = tex2D(_MainTex, float2(i.uv.x, 0.5)).r;
					col.r = lerp(0, pow(val, 0.5), smoothstep(0.05, -0.05, i.uv.y - val));

					val = tex2D(_MainTex, float2(i.uv.x, 0.5)).g;
					col.g = lerp(0, pow(val, 0.5), smoothstep(0.05, -0.05, i.uv.y - val));

					val = tex2D(_MainTex, float2(i.uv.x, 0.5)).b;
					col.b = lerp(0, pow(val, 0.5), smoothstep(0.05, -0.05, i.uv.y - val));
				}
				 
				 //col.r = (frac(i.uv.x * 4));

                return col;
            }
            ENDCG
        }
    }
}
