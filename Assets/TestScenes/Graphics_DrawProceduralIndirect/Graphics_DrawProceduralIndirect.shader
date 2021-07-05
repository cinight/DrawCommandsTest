Shader "Unlit/Graphics_DrawProceduralIndirect"
{
    Properties
    {
        _Size ("_Size", Range(0,2)) = 1
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos : POSITION;
            };

            struct v2g
            {
                float4 pos : SV_POSITION;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            #if SHADER_TARGET >= 45
                StructuredBuffer<float4> positionBuffer;
            #endif

            float _Size;

            v2g vert (appdata v, uint instanceID : SV_InstanceID)
            {
                #if SHADER_TARGET >= 45
                    float4 data = positionBuffer[instanceID];
                #else
                    float4 data = 0;
                #endif

                float3 wPos = data.xyz + v.pos.xyz;

                v2g o;
                o.pos = mul(UNITY_MATRIX_VP, float4(wPos, 1.0f));
                return o;
            }

            [maxvertexcount(3)]
            void geom (point v2g input[1], inout TriangleStream<g2f> outStream)
            {
                float dx = _Size;
                float dy = _Size * _ScreenParams.x / _ScreenParams.y;
                g2f output;
                output.pos = input[0].pos + float4(-dx, dy,0,0); output.uv=float2(0,0); outStream.Append (output);
                output.pos = input[0].pos + float4( dx, dy,0,0); output.uv=float2(1,0); outStream.Append (output);
                output.pos = input[0].pos + float4(-dx,-dy,0,0); output.uv=float2(0,1); outStream.Append (output);
                outStream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
