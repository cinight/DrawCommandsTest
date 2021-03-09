Shader "Unlit/Graphics_DrawMeshInstancedIndirect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            #if SHADER_TARGET >= 45
                StructuredBuffer<float3> positionBuffer;
            #endif

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                #if SHADER_TARGET >= 45
                    float3 data = positionBuffer[instanceID];
                #else
                    float3 data = 0;
                #endif

                float3 wPos = data.xyz + v.vertex.xyz;

                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(wPos, 1.0f));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
