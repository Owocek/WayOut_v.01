using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FPCameraGlobals : MonoBehaviour
{
    public float depthEpsilon = 0.01f;
    Camera _cam;

    void OnEnable() { _cam = GetComponent<Camera>(); }

    void LateUpdate()
    {
        if (!_cam) return;
        var projGPU = GL.GetGPUProjectionMatrix(_cam.projectionMatrix, false); // renderToTexture = false
        var view = _cam.worldToCameraMatrix;
        var vp = projGPU * view;
        var invVP = vp.inverse;

        Shader.SetGlobalMatrix("_FPCameraVP", vp);
        Shader.SetGlobalMatrix("_FPInvVP", invVP);
        Shader.SetGlobalMatrix("_FPWorldToView", view);
        Shader.SetGlobalFloat("_DepthEpsilon", depthEpsilon);
    }
}