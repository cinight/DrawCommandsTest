// https://docs.unity3d.com/6000.0/Documentation/Manual/gpu-instancing-vertex-fragment-shader-example.html
Shader "Custom/SimplestInstancedShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _ColorList[64];

            // UNITY_INSTANCING_BUFFER_START(Props)
            // UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            // UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 col = tex2D(_MainTex, i.uv);

                //float4 instanceColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                #if defined(UNITY_INSTANCING_ENABLED)
                uint id = UNITY_GET_INSTANCE_ID(i);
                col *= _ColorList[id];
                #endif
                
                return col;
            }
            ENDCG
        }
    }
}
