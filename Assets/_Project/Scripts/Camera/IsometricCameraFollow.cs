using UnityEngine;

public sealed class IsometricCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(-10f, 14f, -10f);
    [SerializeField, Min(0f)] private float smoothTime = 0.15f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position + offset,
            ref velocity,
            smoothTime);

        transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);
    }
}
