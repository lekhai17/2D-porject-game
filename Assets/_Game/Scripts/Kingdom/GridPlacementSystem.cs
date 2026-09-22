using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomRuins
{
    /// <summary>
    /// Dat cong trinh tren luoi o Kingdom View. Giu mot bang o da chiem
    /// de chan dat chong len nhau. Ho tro ghost preview theo chuot.
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public class GridPlacementSystem : MonoBehaviour
    {
        public static GridPlacementSystem Instance { get; private set; }

        [Header("Vung dat duoc (tinh theo o, goc duoi-trai)")]
        [SerializeField] private Vector2Int areaSize = new Vector2Int(20, 20);
        [SerializeField] private Vector2Int areaOrigin = Vector2Int.zero;

        [Header("Preview")]
        [SerializeField] private GameObject ghostPrefab;
        [SerializeField] private Color validColor = new Color(0.4f, 1f, 0.4f, 0.6f);
        [SerializeField] private Color invalidColor = new Color(1f, 0.35f, 0.35f, 0.6f);

        [Header("Prefab cong trinh mac dinh")]
        [Tooltip("Prefab rong co SpriteRenderer + ProductionBuilding, dung cho moi BuildingData")]
        [SerializeField] private GameObject buildingPrefab;

        [Header("Camera dung de doc vi tri chuot")]
        [SerializeField] private Camera kingdomCamera;

        private Grid grid;
        private readonly Dictionary<Vector3Int, ProductionBuilding> occupied = new Dictionary<Vector3Int, ProductionBuilding>();
        private BuildingData selected;
        private GameObject ghost;
        private SpriteRenderer ghostRenderer;

        public BuildingData Selected => selected;
        public bool IsPlacing => selected != null;

        private void Awake()
        {
            Instance = this;
            grid = GetComponent<Grid>();
            if (kingdomCamera == null) kingdomCamera = Camera.main;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            // Chi cho xay trong pha Kingdom
            if (gm != null && gm.Phase != GamePhase.Kingdom)
            {
                if (IsPlacing) CancelPlacement();
                return;
            }

            if (!IsPlacing) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector3Int cell = ScreenToCell(mouse.position.ReadValue());
            bool ok = CanPlace(selected, cell);
            UpdateGhost(cell, ok);

            // Bam vao bang chon o duoi thi khong tinh la dat cong trinh
            bool tренUI = UnityEngine.EventSystems.EventSystem.current != null
                       && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (mouse.leftButton.wasPressedThisFrame && ok && !tренUI)
                Place(selected, cell);

            if (mouse.rightButton.wasPressedThisFrame ||
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
                CancelPlacement();
        }

        // ---------- Chon / huy ----------

        public void BeginPlacement(BuildingData buildingData)
        {
            if (buildingData == null) return;
            selected = buildingData;
            EnsureGhost();
        }

        public void CancelPlacement()
        {
            selected = null;
            if (ghost != null) ghost.SetActive(false);
        }

        // ---------- Kiem tra & dat ----------

        public bool CanPlace(BuildingData buildingData, Vector3Int origin)
        {
            if (buildingData == null) return false;

            // Du tai nguyen?
            if (ResourceManager.Instance != null && buildingData.buildCosts != null &&
                !ResourceManager.Instance.CanAfford(buildingData.buildCosts))
                return false;

            var size = buildingData.footprint;
            for (int x = 0; x < Mathf.Max(1, size.x); x++)
            {
                for (int y = 0; y < Mathf.Max(1, size.y); y++)
                {
                    var c = new Vector3Int(origin.x + x, origin.y + y, 0);
                    if (!IsInsideArea(c)) return false;
                    if (occupied.ContainsKey(c)) return false;
                }
            }
            return true;
        }

        public ProductionBuilding Place(BuildingData buildingData, Vector3Int origin)
        {
            if (!CanPlace(buildingData, origin)) return null;

            if (ResourceManager.Instance != null && buildingData.buildCosts != null &&
                buildingData.buildCosts.Length > 0 &&
                !ResourceManager.Instance.TrySpend(buildingData.buildCosts))
                return null;

            GameObject go;
            if (buildingPrefab != null)
            {
                go = Instantiate(buildingPrefab, transform);
            }
            else
            {
                go = new GameObject("Building");
                go.transform.SetParent(transform);
                go.AddComponent<SpriteRenderer>();
                go.AddComponent<ProductionBuilding>();
            }

            var size = buildingData.footprint;
            // Dat vao giua vung chiem dung
            Vector3 world = grid.GetCellCenterWorld(origin);
            world += new Vector3((Mathf.Max(1, size.x) - 1) * grid.cellSize.x * 0.5f,
                                 (Mathf.Max(1, size.y) - 1) * grid.cellSize.y * 0.5f, 0f);
            go.transform.position = world;
            go.name = "Building_" + buildingData.displayName;

            var pb = go.GetComponent<ProductionBuilding>();
            if (pb == null) pb = go.AddComponent<ProductionBuilding>();
            pb.Initialize(buildingData, origin);

            for (int x = 0; x < Mathf.Max(1, size.x); x++)
                for (int y = 0; y < Mathf.Max(1, size.y); y++)
                    occupied[new Vector3Int(origin.x + x, origin.y + y, 0)] = pb;

            CancelPlacement();
            return pb;
        }

        public void Demolish(ProductionBuilding pb)
        {
            if (pb == null) return;

            var keys = new List<Vector3Int>();
            foreach (var kv in occupied)
                if (kv.Value == pb) keys.Add(kv.Key);
            foreach (var k in keys) occupied.Remove(k);

            pb.UnassignAll();
            Destroy(pb.gameObject);
        }

        public ProductionBuilding GetBuildingAt(Vector3Int cell)
            => occupied.TryGetValue(cell, out var pb) ? pb : null;

        // ---------- Tien ich ----------

        public Vector3Int ScreenToCell(Vector2 screenPos)
        {
            var cam = kingdomCamera != null ? kingdomCamera : Camera.main;
            if (cam == null) return Vector3Int.zero;

            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            world.z = 0f;
            return grid.WorldToCell(world);
        }

        public bool IsInsideArea(Vector3Int cell)
            => cell.x >= areaOrigin.x && cell.x < areaOrigin.x + areaSize.x
            && cell.y >= areaOrigin.y && cell.y < areaOrigin.y + areaSize.y;

        private void EnsureGhost()
        {
            if (ghost == null)
            {
                ghost = ghostPrefab != null
                    ? Instantiate(ghostPrefab, transform)
                    : new GameObject("PlacementGhost");
                if (ghost.transform.parent != transform) ghost.transform.SetParent(transform);
                ghostRenderer = ghost.GetComponent<SpriteRenderer>();
                if (ghostRenderer == null) ghostRenderer = ghost.AddComponent<SpriteRenderer>();
                ghostRenderer.sortingOrder = 100;
            }
            ghost.SetActive(true);
            if (ghostRenderer != null && selected != null && selected.sprite != null)
                ghostRenderer.sprite = selected.sprite;
        }

        private void UpdateGhost(Vector3Int cell, bool valid)
        {
            if (ghost == null) return;

            var size = selected.footprint;
            Vector3 world = grid.GetCellCenterWorld(cell);
            world += new Vector3((Mathf.Max(1, size.x) - 1) * grid.cellSize.x * 0.5f,
                                 (Mathf.Max(1, size.y) - 1) * grid.cellSize.y * 0.5f, 0f);
            ghost.transform.position = world;

            if (ghostRenderer != null)
                ghostRenderer.color = valid ? validColor : invalidColor;
        }

        private void OnDrawGizmosSelected()
        {
            var g = GetComponent<Grid>();
            if (g == null) return;

            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.5f);
            Vector3 a = g.CellToWorld(new Vector3Int(areaOrigin.x, areaOrigin.y, 0));
            Vector3 b = g.CellToWorld(new Vector3Int(areaOrigin.x + areaSize.x, areaOrigin.y + areaSize.y, 0));
            Gizmos.DrawWireCube((a + b) * 0.5f, b - a);
        }
    }
}
