using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory), typeof(PlayerInput))]
public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField, Min(0f)] private float interactionRadius = 2.5f;
    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnInteract(InputValue value)
    {
        if (value.isPressed) Interact();
    }

    public bool Interact()
    {
        if (inventory == null || interactionRadius <= 0f) return false;

        ResourceNode nearest = null;
        float nearestDistance = interactionRadius * interactionRadius;
        foreach (Collider candidate in Physics.OverlapSphere(transform.position, interactionRadius))
        {
            ResourceNode node = candidate.GetComponentInParent<ResourceNode>();
            if (node == null || !node.CanGather) continue;

            float distance = (node.transform.position - transform.position).sqrMagnitude;
            if (distance > nearestDistance) continue;
            nearestDistance = distance;
            nearest = node;
        }

        return nearest != null && nearest.Gather(inventory);
    }
}
