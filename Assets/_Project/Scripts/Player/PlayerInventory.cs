using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    [SerializeField, Min(0)] private int wood;
    [SerializeField, Min(0)] private int stone;

    public int Wood => wood;
    public int Stone => stone;

    public bool AddResource(ResourceType type, int amount)
    {
        if (amount < 0) return false;
        switch (type)
        {
            case ResourceType.Wood:
                if (amount > int.MaxValue - wood) return false;
                wood += amount;
                return true;
            case ResourceType.Stone:
                if (amount > int.MaxValue - stone) return false;
                stone += amount;
                return true;
            default:
                return false;
        }
    }

    public bool HasResource(ResourceType type, int amount)
    {
        if (amount < 0) return false;
        return type switch
        {
            ResourceType.Wood => wood >= amount,
            ResourceType.Stone => stone >= amount,
            _ => false
        };
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (!HasResource(type, amount)) return false;
        if (type == ResourceType.Wood) wood -= amount;
        else stone -= amount;
        return true;
    }

    private void OnValidate()
    {
        wood = Mathf.Max(0, wood);
        stone = Mathf.Max(0, stone);
    }
}
