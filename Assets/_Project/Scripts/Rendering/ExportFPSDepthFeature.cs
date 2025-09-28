using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class ExportFPSDepthFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings { public Shader copyDepthShader; }
    public Settings settings = new Settings();

    class Pass : ScriptableRenderPass
    {
        static readonly int FPSDepthTexID = Shader.PropertyToID("_FPSDepthTexture");

        Material _copyMat;
        RTHandle _fpsDepthCopy;
        ProfilingSampler _sampler = new ProfilingSampler("Export FPS Depth");

        public Pass(Shader copyDepthShader)
        {
            renderPassEvent = RenderPassEvent.AfterRendering; // po renderze bieżącej kamery
            if (copyDepthShader != null) _copyMat = CoreUtils.CreateEngineMaterial(copyDepthShader);
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        [System.Obsolete("URP15 compatibility override")]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.graphicsFormat = GraphicsFormat.R32_SFloat;

            // Krótszy overload bez dynamicResolution/useMipMap/aniso/mipMapBias
            RenderingUtils.ReAllocateHandleIfNeeded(
                ref _fpsDepthCopy, desc,
                FilterMode.Point, TextureWrapMode.Clamp,
                name: "_FPSDepthCopy"
            );
        }

        [System.Obsolete("URP15 compatibility override")]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cam = renderingData.cameraData.camera;
            if (!cam || !cam.TryGetComponent<FPCameraTag>(out _)) return;
            if (_copyMat == null) return;

            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _sampler))
            {
                CoreUtils.SetRenderTarget(cmd, _fpsDepthCopy);
                CoreUtils.DrawFullScreen(cmd, _copyMat);
                cmd.SetGlobalTexture(FPSDepthTexID, _fpsDepthCopy);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    Pass _pass;

    public override void Create()
    {
        _pass = new Pass(settings.copyDepthShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}