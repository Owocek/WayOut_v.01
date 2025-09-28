Shader "Hidden/Custom/CopyDepth"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "CopyDepthToColor"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // _CameraDepthTexture jest deklarowana przez DeclareDepthTexture.hlsl
            // TEXTURE2D_X_FLOAT(_CameraDepthTexture); // <- usunięte
            // SAMPLER(sampler_CameraDepthTexture);    // <- usunięte

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings  { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = float4(IN.positionOS.xy, 0, 1);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 Frag (Varyings IN) : SV_Target
            {
                float d = SampleSceneDepth(IN.uv);
                return float4(d, d, d, 1);
            }
            ENDHLSL
        }
    }
}