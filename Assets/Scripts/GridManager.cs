using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{

    [Header("Camera Framing")]
    public Transform cameraPivot;
    public Camera mainCamera;
    public float cameraPadding = 1f; // extra breathing room

    [Header("Grid Data")]

    public int width = 7;
    public int height = 7;
    float tileSize = 1f;
    public GameObject tilePrefab;

    [SerializeField] GameObject teleportEffect;

    private GameObject[,] grid;

    // ✅ RUNTIME ITEMS (actual instances)
    public List<Item> items = new List<Item>();

    // ✅ SETUP ITEMS (inspector-driven)

    private LevelData currentLevel;

    void CenterCameraOnGrid()
    {
        if (mainCamera == null || cameraPivot == null)
            return;

        // --- ORTHOGRAPHIC SIZE ---
        float dominant = (float)Mathf.Max(width, height);

        // This exactly matches:
        // 1×1 → 1
        // 4×4 → 2
        // 5×1 → 2
        float ortho = dominant / 2f;
        mainCamera.orthographicSize = ortho;

        // --- PIVOT POSITION ---

        float diff = width - height;

        float pivotX = diff * 0.5f - 0.5f;
        float pivotZ = diff > 0 ? diff * 0.5f : -0.5f;
        float pivotY = ortho * 1.0f;

        cameraPivot.position = new Vector3(
            pivotX,
            pivotY,
            pivotZ
        );
    }




    public void LoadLevel(LevelData level)
    {
        currentLevel = level;

        ClearGrid();
        GenerateGrid(level.width, level.height);
        SpawnSetupItems(level.setUpItems);
        CenterCameraOnGrid();

    }
    void GenerateGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos =
                    GridToWorld(x, y, -0.5f * tilePrefab.transform.localScale.y);

                GameObject tile = Instantiate(tilePrefab, worldPos, Quaternion.identity);
                tile.name = $"Tile ({x},{y})";
                tile.transform.parent = this.transform;

                grid[x, y] = tile;
            }
        }
    }

    public Vector3 GridToWorld(int x, int y, float z = 0.5f)
    {
        return new Vector3(
            x * tileSize,
            z,
            y * tileSize
        );
    }

    void ClearGrid()
    {
        if (grid != null)
        {
            foreach (GameObject tile in grid)
            {
                if (tile != null)
                    Destroy(tile);
            }
        }

        foreach (Item item in items)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        items.Clear();
    }

    // ===============================
    // ✅ ITEM SYSTEM
    // ===============================

    void SpawnSetupItems(List<ItemSetup> setUpItems)
    {
        items.Clear();

        foreach (ItemSetup setup in setUpItems)
        {

            Item instance = Instantiate(setup.itemPrefab, transform);


            instance.Initialize(setup.x, setup.y);



            instance.transform.position =
                GridToWorld(setup.x, setup.y,
                instance.transform.localScale.y / 2);

            RegisterItem(instance);

            if (instance is Empty)
            {
                RemoveTileAt(setup.x, setup.y);
            }
        }
    }


    public void RegisterItem(Item item)
    {
        items.Add(item);
    }

    public Item GetItemAt(int x, int y)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].x == x && items[i].y == y)
                return items[i];
        }

        return null;
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public void RemoveTileAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        GameObject tile = grid[x, y];

        if (tile != null)
        {
            Destroy(tile);
            grid[x, y] = null;
        }
    }

}
