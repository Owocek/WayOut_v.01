Shader "Hidden/Custom/TopDownMaskByFPS"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "MaskByFPS"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // Kolor źródłowy (Blitter ustawia _BlitTexture)
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // Głębia top-down jest deklarowana przez DeclareDepthTexture.hlsl (SampleSceneDepth)
            // TEXTURE2D_X_FLOAT(_CameraDepthTexture);  // <- usunięte
            // SAMPLER(sampler_CameraDepthTexture);     // <- usunięte

            // Głębia z FPS (kopiowana w feature)
            TEXTURE2D_X_FLOAT(_FPSDepthTexture);
            SAMPLER(sampler_FPSDepthTexture);

            float4x4 _FPCameraVP;
            float4x4 _FPInvVP;
            float4x4 _FPWorldToView;
            float    _DepthEpsilon;

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings  { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = float4(IN.positionOS.xy, 0, 1);
                OUT.uv = IN.uv;
                return OUT;
            }

            float3 WorldPosFromTopDown(float2 uv)
            {
                float d = SampleSceneDepth(uv);
                float3 worldPos = ComputeWorldSpacePosition(uv, d, UNITY_MATRIX_I_VP);
                return worldPos;
            }

            float3 WorldPosFromFPSDepth(float2 uvFPS)
            {
                float d = SAMPLE_TEXTURE2D_X(_FPSDepthTexture, sampler_FPSDepthTexture, uvFPS).r;
                #if defined(UNITY_REVERSED_Z)
                    float zNDC = (1.0 - d) * 2.0 - 1.0;
                #else
                    float zNDC = d * 2.0 - 1.0;
                #endif
                float4 clip = float4(uvFPS * 2.0 - 1.0, zNDC, 1.0);
                float4 wpos = mul(_FPInvVP, clip);
                return wpos.xyz / wpos.w;
            }

            bool IsVisibleFromFPS(float3 worldPos)
            {
                float4 clipFPS = mul(_FPCameraVP, float4(worldPos, 1.0));
                if (clipFPS.w <= 0) return false;

                float2 ndc = clipFPS.xy / clipFPS.w;
                float2 uvFPS = ndc * 0.5 + 0.5;
                if (any(uvFPS < 0.0) || any(uvFPS > 1.0)) return false;

                float3 viewPosFrag = mul(_FPWorldToView, float4(worldPos, 1.0)).xyz;
                float  eyeDepthFrag = -viewPosFrag.z;

                float3 worldFromFPSDepth = WorldPosFromFPSDepth(uvFPS);
                float3 viewPosScene = mul(_FPWorldToView, float4(worldFromFPSDepth, 1.0)).xyz;
                float  eyeDepthScene = -viewPosScene.z;

                return (eyeDepthFrag <= eyeDepthScene + _DepthEpsilon);
            }

            float4 Frag (Varyings IN) : SV_Target
            {
                float dTop = SampleSceneDepth(IN.uv);
                if (dTop >= 0.999999) return float4(0,0,0,0);

                float3 wpos = WorldPosFromTopDown(IN.uv);
                float4 col  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, IN.uv);

                bool visible = IsVisibleFromFPS(wpos);
                return visible ? col : float4(col.rgb, 0.0);
            }
            ENDHLSL
        }
    }
}