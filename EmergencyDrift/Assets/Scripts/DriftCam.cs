using UnityEngine;

public class DriftCam : MonoBehaviour
{
    public Transform target;
    private Rigidbody rb;

    [Header("Camera Settings")]
    public float distance = 6f;
    public float height = 3f;
    public float followSmooth = 8f;
    public float rotationSmooth = 8f;

    [Header("Collision")]
    public LayerMask collisionMask;

    private bool ready = false;

    private void Start()
    {
        rb = target.GetComponent<Rigidbody>();

        // Stable starting position
        Vector3 startPos = target.position
                           - target.forward * distance
                           + Vector3.up * height;

        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation(target.forward, Vector3.up);

        // Wait 1 frame so Unity finishes all transforms

        ready = true;
    }

    void LateUpdate()
    {
        if (!ready || !target) return;

        // --- DESIRED POSITION ---
        Vector3 desiredPos = target.position
                             - target.forward * distance
                             + Vector3.up * height;

        // --- COLLISION CHECK ---
        if (Physics.Linecast(target.position + Vector3.up * 1.5f, desiredPos, out RaycastHit hit, collisionMask))
        {
            desiredPos = hit.point + hit.normal * 0.5f;
        }

        // --- SMOOTH POSITION ---
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        // --- SAFE LOOK DIRECTION ---
        Vector3 lookDir = (target.position + Vector3.up * 1.5f) - transform.position;

        // Prevent vertical look direction (causes flips)
        if (Mathf.Abs(Vector3.Dot(lookDir.normalized, Vector3.up)) > 0.95f)
            lookDir.y = 0;

        Quaternion desiredRot = Quaternion.LookRotation(lookDir, Vector3.up);

        // --- SMOOTH ROTATION ---
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);
    }
}

