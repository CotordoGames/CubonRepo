using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Rigidbody2D targetRb; // optional but recommended

    [Header("Follow Settings")]
    public float smoothTime = 0.12f;
    public Vector2 offset;
    
    [Header("Look Ahead")]
    public float lookAheadFactor = 0.35f;
    public float maxLookAhead = 3f;
    public float returnSpeed = 5f;

    [Header("Pixel Perfect")]
    public float pixelsPerUnit = 16f;

    private Vector2 velocity;
    private Vector2 currentLookAhead;
    private Vector2 smoothVelocity;

    void LateUpdate()
    {
        if (!target) return;

        Vector2 targetPos = target.position;

        // --- 1. Get velocity (prefer Rigidbody2D, fallback to manual delta)
        Vector2 vel = targetRb ? targetRb.linearVelocity :
            (Vector2)target.position - (Vector2)transform.position;

        // --- 2. Compute look-ahead
        Vector2 desiredLookAhead = Vector2.ClampMagnitude(vel * lookAheadFactor, maxLookAhead);

        currentLookAhead = Vector2.Lerp(
            currentLookAhead,
            desiredLookAhead,
            Time.deltaTime * returnSpeed
        );

        // --- 3. Desired camera position
        Vector2 desiredPos = targetPos + currentLookAhead + offset;

        // --- 4. Smooth follow
        Vector2 smoothed = Vector2.SmoothDamp(
            transform.position,
            desiredPos,
            ref smoothVelocity,
            smoothTime
        );

        // --- 5. Pixel snap (CRUCIAL for crispness)
        Vector2 snapped = SnapToPixel(smoothed);

        transform.position = new Vector3(snapped.x, snapped.y, transform.position.z);
    }

    Vector2 SnapToPixel(Vector2 pos)
    {
        float ppu = pixelsPerUnit;

        float x = Mathf.Round(pos.x * ppu) / ppu;
        float y = Mathf.Round(pos.y * ppu) / ppu;

        return new Vector2(x, y);
    }
}