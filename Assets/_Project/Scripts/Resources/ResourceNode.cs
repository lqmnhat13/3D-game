using UnityEngine;

public sealed class ResourceNode : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField, Min(1)] private int amount = 1;
    private bool collected;

    public bool CanGather => isActiveAndEnabled && !collected && amount > 0
        && (resourceType == ResourceType.Wood || resourceType == ResourceType.Stone);

    public bool Gather(PlayerInventory inventory)
    {
        if (!CanGather || inventory == null || !inventory.AddResource(resourceType, amount))
            return false;

        collected = true;
        gameObject.SetActive(false);
        return true;
    }

    public void ResetForDay()
    {
        collected = false;
        gameObject.SetActive(true);
    }
}
