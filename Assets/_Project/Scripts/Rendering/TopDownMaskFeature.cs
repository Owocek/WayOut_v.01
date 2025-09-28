using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TopDownMaskFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings { public Material maskMaterial; }
    public Settings settings = new Settings();

    class Pass : ScriptableRenderPass
    {
        readonly Material _mat;
        RTHandle _tempColor;
        ProfilingSampler _sampler = new ProfilingSampler("TopDown Mask By FPS");

        public Pass(Material mat)
        {
            _mat = mat;
            renderPassEvent = RenderPassEvent.AfterRendering; // po renderze top-down
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        [System.Obsolete("URP15 compatibility override")]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateHandleIfNeeded(
                ref _tempColor, desc,
                FilterMode.Bilinear, TextureWrapMode.Clamp,
                name: "_TopDownTempColor"
            );
        }

        [System.Obsolete("URP15 compatibility override")]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cam = renderingData.cameraData.camera;
            if (!cam || !cam.TryGetComponent<TopDownCameraTag>(out _)) return;
            if (_mat == null) return;

            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _sampler))
            {
#pragma warning disable 0618
                var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
#pragma warning restore 0618

                // source -> temp
                Blitter.BlitCameraTexture(cmd, source, _tempColor);

                // temp -> source z materiałem (pass 0)
                // UWAGA: tu była przyczyna błędu – potrzeba 5. parametru (passIndex)
                Blitter.BlitCameraTexture(cmd, _tempColor, source, _mat, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    Pass _pass;

    public override void Create()
    {
        _pass = new Pass(settings.maskMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}