using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory), typeof(PlayerInput))]
public sealed class BuildingSystem : MonoBehaviour
{
    [SerializeField] private PlaceableBuilding fencePrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Collider ground;
    [SerializeField] private HearthController hearth;
    [SerializeField, Min(0f)] private float clearance = 0.1f;
    private PlayerInventory inventory;
    private Renderer hearthVisual;
    private GameObject preview;
    private Renderer[] previewRenderers;
    private MaterialPropertyBlock previewColor;
    private Vector3 center, halfSize;
    private readonly Collider[] overlaps = new Collider[32];

    public bool IsBuildMode { get; private set; }

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (fencePrefab == null || ground == null || hearth == null || mainCamera == null)
        {
            Debug.LogError("BuildingSystem requires Fence prefab, Ground, Hearth and Main Camera.", this);
            enabled = false;
            return;
        }
        hearthVisual = hearth.GetComponent<Renderer>();
        var box = fencePrefab.GetComponent<BoxCollider>();
        center = box.center;
        halfSize = box.size * 0.5f;
        previewColor = new MaterialPropertyBlock();
    }

    private void OnBuild(InputValue value)
    {
        if (value.isPressed && enabled) SetBuildMode(!IsBuildMode);
    }

    private void OnAttack(InputValue value)
    {
        if (value.isPressed && IsBuildMode && Mouse.current != null)
            TryPlace(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
    }

    public void SetBuildMode(bool active)
    {
        if (!enabled) return;
        IsBuildMode = active;
        if (active && preview == null)
        {
            // Only copy the visual child: no collider, Fence or NavMeshObstacle in the preview.
            preview = Instantiate(fencePrefab.Visuals.gameObject);
            preview.name = "Fence Preview";
            previewRenderers = preview.GetComponentsInChildren<Renderer>();
        }
        if (preview != null) preview.SetActive(active);
    }

    private void Update()
    {
        if (!IsBuildMode || Mouse.current == null) return;
        bool onGround = TryGetPosition(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out var position);
        preview.SetActive(onGround);
        if (!onGround) return;
        preview.transform.position = position;
        bool valid = CanPlace(position) && inventory.HasResource(ResourceType.Wood, fencePrefab.WoodCost);
        previewColor.SetColor("_BaseColor", valid ? Color.green : Color.red);
        foreach (var renderer in previewRenderers) renderer.SetPropertyBlock(previewColor);
    }

    public bool TryPlace(Ray ray)
    {
        if (!enabled || !IsBuildMode || !TryGetPosition(ray, out var position) || !CanPlace(position)
            || !inventory.HasResource(ResourceType.Wood, fencePrefab.WoodCost)) return false;
        var placed = Instantiate(fencePrefab, position, Quaternion.identity);
        if (!inventory.SpendResource(ResourceType.Wood, fencePrefab.WoodCost))
        {
            placed.gameObject.SetActive(false);
            Destroy(placed.gameObject);
            return false;
        }
        Physics.SyncTransforms();
        return true;
    }

    private bool TryGetPosition(Ray ray, out Vector3 position)
    {
        position = default;
        if (!Physics.Raycast(ray, out var hit, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            || hit.collider != ground || hit.normal.y < 0.99f) return false;
        position = new Vector3(Mathf.Round(hit.point.x), hit.point.y, Mathf.Round(hit.point.z));
        return true;
    }

    private bool CanPlace(Vector3 position)
    {
        // Check the whole footprint, including after snapping at the Ground edge.
        for (int x = -1; x <= 1; x += 2)
        for (int z = -1; z <= 1; z += 2)
        {
            var corner = position + new Vector3(center.x + x * halfSize.x, 1f, center.z + z * halfSize.z);
            if (!ground.Raycast(new Ray(corner, Vector3.down), out var hit, 2f)
                || Mathf.Abs(hit.point.y - position.y) > 0.05f) return false;
        }
        var size = halfSize + Vector3.one * clearance;
        var bounds = new Bounds(position + center, size * 2f);
        // Hearth intentionally has no physics collider in the existing scene.
        if (hearthVisual != null && bounds.Intersects(hearthVisual.bounds)) return false;
        int count = Physics.OverlapBoxNonAlloc(position + center, size, overlaps,
            Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (count == overlaps.Length) return false;
        for (int i = 0; i < count; i++)
            if (overlaps[i] != ground) return false;
        return true;
    }

    private void OnDisable()
    {
        IsBuildMode = false;
        if (preview != null) Destroy(preview);
    }
}
