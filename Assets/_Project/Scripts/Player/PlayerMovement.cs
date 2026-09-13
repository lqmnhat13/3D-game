using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(PlayerHealth))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float movementSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private PlayerHealth health;
    private Transform cameraTransform;
    private Vector2 movementInput;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth>();
        cameraTransform = Camera.main.transform;
    }

    private void OnMove(InputValue value)
    {
        movementInput = Vector2.ClampMagnitude(value.Get<Vector2>(), 1f);
    }

    private void Update()
    {
        if (health.IsDead) return;
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 movement = right * movementInput.x + forward * movementInput.y;

        verticalVelocity = characterController.isGrounded && verticalVelocity < 0f
            ? -2f
            : verticalVelocity + gravity * Time.deltaTime;

        characterController.Move((movement * movementSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
    }
}
