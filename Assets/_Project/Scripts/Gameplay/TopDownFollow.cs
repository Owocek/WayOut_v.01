using UnityEngine;

public class TopDownFollow : MonoBehaviour
{
    public Transform target;
    public float height = 25f;
    public Vector2 worldOffsetXZ = Vector2.zero;
    [Range(0f, 1f)] public float smoothFactor = 0.15f;
    public bool lockTopDownRotation = true; // trzymaj X=90, Y=0, Z=0

    Vector3 _vel;

    void LateUpdate()
    {
        if (!target)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform; else return;
        }

        Vector3 desiredPos = new Vector3(
            target.position.x + worldOffsetXZ.x,
            target.position.y + height,
            target.position.z + worldOffsetXZ.y
        );

        if (smoothFactor <= 0f) transform.position = desiredPos;
        else transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, smoothFactor);

        if (lockTopDownRotation) transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}