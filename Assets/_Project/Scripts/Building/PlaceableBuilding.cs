using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Fence))]
public sealed class PlaceableBuilding : MonoBehaviour
{
    [SerializeField, Min(0)] private int woodCost = 5;
    [SerializeField] private Transform visuals;

    public int WoodCost => Mathf.Max(0, woodCost);
    public Transform Visuals => visuals;
}
