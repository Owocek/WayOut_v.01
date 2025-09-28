using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;     
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField, Range(0f, 1f)] private float smoothFactor = 0.15f;
    [SerializeField] private bool useHeadAnchorIfFound = true;

    Vector3 _velocity;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desiredPosition = target.TransformPoint(localOffset); // FPS: offset lokalny względem gracza
        if (smoothFactor <= 0f) transform.position = desiredPosition;
        else transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothFactor);
        // Rotacji kamery nie ruszamy (obsługuje to FirstPersonLook).
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget ? GetBestAnchor(newTarget) : null;
    }

    Transform GetBestAnchor(Transform t)
    {
        if (!useHeadAnchorIfFound) return t;
        var head = t.Find("HeadAnchor");
        return head ? head : t;
    }
}