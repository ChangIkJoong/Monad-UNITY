using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Optional explicit reference. If not set, one will be found at runtime.")]
    [SerializeField] private MonoBehaviour gridManagerBehaviour;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Selection")]
    [SerializeField] private TowerData selectedTower;

    [Header("Preview")]
    [SerializeField, Range(0f, 1f)] private float previewAlpha = 0.7f;
    [SerializeField] private Color canPlacePreviewColor = Color.green;
    [SerializeField] private Color cannotPlacePreviewColor = Color.red;

    private GameObject previewInstance;
    private SpriteRenderer[] previewSpriteRenderers;
    private Color[] previewBaseColors;

    [Header("Placement")]
    [Tooltip("Parent for placed tower instances (optional).")]
    [SerializeField] private Transform placedTowersParent;

    private IGridOccupancy gridOccupancy;

    private void Awake()
    {
        ResolveGridOccupancy();
        if (grid == null) grid = FindFirstObjectByType<Grid>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (groundTilemap == null)
        {
            var allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (var tm in allTilemaps)
            {
                if (tm != null && tm.name == "Ground")
                {
                    groundTilemap = tm;
                    break;
                }
            }

            if (groundTilemap == null && allTilemaps.Length > 0)
            {
                groundTilemap = allTilemaps[0];
            }
        }
    }

    private void ResolveGridOccupancy()
    {
        if (gridOccupancy != null)
        {
            return;
        }

        if (gridManagerBehaviour != null)
        {
            gridOccupancy = gridManagerBehaviour as IGridOccupancy;
            if (gridOccupancy != null)
            {
                return;
            }
        }

        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var b in behaviours)
        {
            if (b is IGridOccupancy occ)
            {
                gridOccupancy = occ;
                return;
            }
        }
    }

    public void SelectTower(TowerData tower)
    {
        selectedTower = tower;
        DestroyPreview();
        EnsurePreviewExists();
    }

    private void Update()
    {
        if (selectedTower == null)
        {
            DestroyPreview();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            ClearSelection();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (TryPlaceSelectedAtMouse())
            {
                ClearSelection();
            }
        }
    }

    private void LateUpdate()
    {
        if (selectedTower == null)
        {
            return;
        }

        EnsurePreviewExists();
        UpdatePreviewAtMouse();
    }

    private bool TryPlaceSelectedAtMouse()
    {
        if (mainCamera == null || groundTilemap == null)
        {
            return false;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        if (selectedTower == null || selectedTower.TowerPrefab == null)
        {
            return false;
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        Vector3Int anchorCell = groundTilemap.WorldToCell(world);
        Vector3Int originCell = GetOriginCellFromAnchor(anchorCell, selectedTower.Footprint);
        Vector3 placementWorldPos = GetFootprintCenterWorld(originCell, selectedTower.Footprint);

        if (!IsWithinPlacementRange(placementWorldPos))
        {
            return false;
        }

        if (!CanPlaceAt(originCell, selectedTower.Footprint))
        {
            return false;
        }

        if (resourceManager != null && !resourceManager.TrySpend(selectedTower.Cost))
        {
            return false;
        }

        GameObject placed = Instantiate(
            selectedTower.TowerPrefab,
            placementWorldPos,
            Quaternion.identity,
            placedTowersParent
        );

        var placedTower = placed.GetComponent<PlacedTower>();
        if (placedTower == null)
        {
            placedTower = placed.AddComponent<PlacedTower>();
        }
        placedTower.Initialize(selectedTower, originCell, selectedTower.Footprint);

        ResolveGridOccupancy();
        if (gridOccupancy != null)
        {
            var occupant = placed.GetComponent<GridOccupant>();
            if (occupant == null)
            {
                occupant = placed.AddComponent<GridOccupant>();
            }
            occupant.Configure(GridObjectKind.Tower, selectedTower.Footprint);

            if (occupant.IsRegistered)
            {
                gridOccupancy.Unregister(occupant);
            }
            gridOccupancy.TryRegister(occupant, originCell, selectedTower.Footprint);
        }

        return true;
    }
    private static Vector3Int GetOriginCellFromAnchor(Vector3Int anchorCell, Vector2Int footprint)
    {
        int dx = Mathf.Max(0, (footprint.x - 1) / 2);
        int dy = Mathf.Max(0, (footprint.y - 1) / 2);
        return anchorCell - new Vector3Int(dx, dy, 0);
    }

    private bool CanPlaceAt(Vector3Int originCell, Vector2Int footprint)
    {
        if (footprint.x < 1 || footprint.y < 1)
        {
            return false;
        }

        ResolveGridOccupancy();
        if (gridOccupancy == null)
        {
            return false;
        }
        return gridOccupancy.CanOccupyFootprint(originCell, footprint);
    }

    private IEnumerable<Vector3Int> EnumerateFootprintCells(Vector3Int originCell, Vector2Int footprint)
    {
        for (int x = 0; x < footprint.x; x++)
        {
            for (int y = 0; y < footprint.y; y++)
            {
                yield return originCell + new Vector3Int(x, y, 0);
            }
        }
    }

    private Vector3 GetFootprintCenterWorld(Vector3Int originCell, Vector2Int footprint)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var cell in EnumerateFootprintCells(originCell, footprint))
        {
            sum += groundTilemap.GetCellCenterWorld(cell);
            count++;
        }

        if (count == 0)
        {
            return groundTilemap.GetCellCenterWorld(originCell);
        }

        Vector3 avg = sum / count;
        avg.z = 0f;
        return avg;
    }

    private void ClearSelection()
    {
        selectedTower = null;
        DestroyPreview();
    }

    private void EnsurePreviewExists()
    {
        if (previewInstance != null || selectedTower == null || selectedTower.TowerPrefab == null)
        {
            return;
        }

        previewInstance = Instantiate(selectedTower.TowerPrefab, Vector3.zero, Quaternion.identity, transform);
        previewInstance.name = $"{selectedTower.TowerName}_Preview";

        DisablePreviewInteractions(previewInstance);
        CachePreviewRenderers(previewInstance);
    }

    private void DestroyPreview()
    {
        if (previewInstance == null)
        {
            previewSpriteRenderers = null;
            previewBaseColors = null;
            return;
        }

        Destroy(previewInstance);
        previewInstance = null;
        previewSpriteRenderers = null;
        previewBaseColors = null;
    }

    private void CachePreviewRenderers(GameObject root)
    {
        previewSpriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        previewBaseColors = new Color[previewSpriteRenderers.Length];
        for (int i = 0; i < previewSpriteRenderers.Length; i++)
        {
            previewBaseColors[i] = previewSpriteRenderers[i].color;
        }
    }

    private void DisablePreviewInteractions(GameObject root)
    {
        foreach (var animator in root.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
        foreach (var animation in root.GetComponentsInChildren<Animation>(true))
        {
            animation.enabled = false;
        }

        foreach (var col in root.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }
        foreach (var col2D in root.GetComponentsInChildren<Collider2D>(true))
        {
            col2D.enabled = false;
        }

        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        foreach (var rb2D in root.GetComponentsInChildren<Rigidbody2D>(true))
        {
            rb2D.simulated = false;
        }

        foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            mb.enabled = false;
        }

        SetLayerRecursively(root, 2); // Ignore Raycast
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        var t = obj.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }

    private void UpdatePreviewAtMouse()
    {
        if (previewInstance == null || mainCamera == null || groundTilemap == null || selectedTower == null)
        {
            return;
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        Vector3Int anchorCell = groundTilemap.WorldToCell(world);
        Vector3Int originCell = GetOriginCellFromAnchor(anchorCell, selectedTower.Footprint);
        Vector3 placementWorldPos = GetFootprintCenterWorld(originCell, selectedTower.Footprint);
        previewInstance.transform.position = placementWorldPos;

        bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool canAfford = resourceManager == null || resourceManager.CanAfford(selectedTower.Cost);
        bool withinRange = IsWithinPlacementRange(placementWorldPos);
        bool canPlace = !pointerOverUi && canAfford && withinRange && CanPlaceAt(originCell, selectedTower.Footprint);

        ApplyPreviewTint(canPlace ? canPlacePreviewColor : cannotPlacePreviewColor);
    }

    private bool IsWithinPlacementRange(Vector3 placementWorldPos)
    {
        if (playerMovement == null)
        {
            return true;
        }

        float range = Mathf.Max(0f, playerMovement.PlacementRange);
        Vector2 playerPos = playerMovement.transform.position;
        Vector2 placePos = placementWorldPos;
        return (placePos - playerPos).sqrMagnitude <= range * range;
    }

    private void ApplyPreviewTint(Color tint)
    {
        if (previewSpriteRenderers == null || previewBaseColors == null)
        {
            return;
        }

        float a = Mathf.Clamp01(previewAlpha);
        for (int i = 0; i < previewSpriteRenderers.Length; i++)
        {
            if (previewSpriteRenderers[i] == null)
            {
                continue;
            }

            Color baseColor = previewBaseColors[i];
            previewSpriteRenderers[i].color = new Color(
                baseColor.r * tint.r,
                baseColor.g * tint.g,
                baseColor.b * tint.b,
                baseColor.a * a
            );
        }
    }
}
