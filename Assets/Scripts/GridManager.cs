using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [Header("Grid Spawn Animation")]
    [SerializeField] float spawnDropDistance = 10f;
    [SerializeField] float tileRiseDuration = 0.3f;
    [SerializeField] float columnDelay = 0.08f;
    [SerializeField] AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Camera Framing")]
    public Transform cameraPivot;
    public Camera mainCamera;
    public float cameraPadding = 1f;

    [Header("Grid Data")]
    public int width = 7;
    public int height = 7;
    float tileSize = 1f;
    public GameObject tilePrefab;

    [SerializeField] GameObject teleportEffect;

    private GameObject[,] grid;

    // ✅ RUNTIME ITEMS (actual instances)
    public List<Item> items = new List<Item>();

    private GameObject playerGameObject;

    // ===============================
    // ✅ CAMERA
    // ===============================

    void CenterCameraOnGrid()
    {
        if (mainCamera == null || cameraPivot == null)
            return;

        float dominant = Mathf.Max(width, height);
        float submissive = Mathf.Min(width, height);
        float ratio = submissive / dominant;
        float ortho = dominant / 2f;
        mainCamera.orthographicSize = ortho;


        float pivotX = 0;
        float pivotZ = 0;
        float pivotY = 0.01399994f;
        cameraPivot.position = new Vector3(pivotX, pivotY, pivotZ);
    }

    // ===============================
    // ✅ LEVEL LOADING
    // ===============================

    public void LoadLevel(LevelData level)
    {
        GetComponent<ThemeApplier>().ApplyColors();
        gameObject.SetActive(false);

        ClearGrid();
        GenerateGrid(level.width, level.height);
        SpawnSetupItems(level.setUpItems);
        CenterCameraOnGrid();

        OffsetAllTilesAndItemsDown();

        gameObject.SetActive(true);

        StartCoroutine(AnimateGridRise());
    }

    IEnumerator AnimateGridRise()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    StartCoroutine(RiseObject(grid[x, y].transform));
                }

                List<Item> tileItems = GetItemsAt(x, y);
                foreach (Item item in tileItems)
                {
                    StartCoroutine(RiseObject(item.transform));
                }
            }

            yield return new WaitForSeconds(columnDelay);
        }
        playerGameObject.GetComponent<PlayerController>().StartPlayerPos();

        StartCoroutine(TeleportItemIn(playerGameObject));
    }

    void OffsetAllTilesAndItemsDown()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].transform.position += Vector3.down * spawnDropDistance;
                }

                List<Item> tileItems = GetItemsAt(x, y);
                foreach (Item item in tileItems)
                {
                    item.transform.position += Vector3.down * spawnDropDistance;
                }
            }
        }
    }

    IEnumerator RiseObject(Transform target)
    {
        Vector3 startPos = target.position;
        Vector3 endPos = startPos + Vector3.up * spawnDropDistance;

        float elapsed = 0f;

        while (elapsed < tileRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tileRiseDuration);
            float eased = riseCurve.Evaluate(t);

            target.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        target.position = endPos;
    }

    // ===============================
    // ✅ GRID
    // ===============================

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
    // ✅ ITEM SYSTEM (MULTI-ITEM SAFE)
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
                instance.transform.localScale.y);

            RegisterItem(instance);

            if (instance is Empty)
            {
                RemoveTileAt(setup.x, setup.y);
            }

            if (instance is PlayerController)
            {
                playerGameObject = instance.gameObject;
                instance.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator TeleportItemIn(GameObject gameObject)
    {
        FindFirstObjectByType<SFXManager>().PlayClip("start");

        gameObject.SetActive(true);
        GetComponent<ThemeApplier>().ApplyColors();

        Instantiate(teleportEffect, gameObject.transform.position, Quaternion.identity);
        yield return null;
    }

    public void RegisterItem(Item item)
    {
        items.Add(item);
    }

    // ✅ NEW: MULTIPLE ITEMS PER TILE
    public List<Item> GetItemsAt(int x, int y)
    {
        List<Item> result = new List<Item>();

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].x == x && items[i].y == y)
                result.Add(items[i]);
        }

        return result;
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
