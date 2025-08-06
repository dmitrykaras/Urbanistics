using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPainter : MonoBehaviour
{
    public static RoadPainter Instance { get; private set; }

    [Header("Основные настройки")]
    public Tilemap roadTilemap; //Tilemap, на которой рисуем дороги
    public TileBase RoadPrefab; //сам тайл дороги (можно RuleTile для авто-тайлинга)
    public Transform RoadGhost; //объект-призрак (спрайт) — optional
    public Camera mainCamera;

    [Header("Настройки")]
    public KeyCode toggleKey = KeyCode.R;
    public int brushSize = 1; //1 = одна клетка, 2 = 3x3 и т.д. (несколько реализаций)
    public float placeInterval = 0.02f; //минимальный интервал между постановками при drag
    public bool requireHoldMouse = true; //true — рисуем только при зажатой ЛКМ

    //ресурсы на тайл
    public ResourceCost[] costPerTile; // если у тебя есть ResourceCost struct

    public bool isPainting = false; //включение и выключение режима строительства дорог
    private float lastPlaceTime = 0f;
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    public bool runningRoadMode = false;

    public GameObject RoadGhostInstance;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        GameObject ghost = Resources.Load<GameObject>("RoadGhost");
        if (ghost != null)
            RoadGhostInstance = Instantiate(ghost);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        RoadGhostRunning(); //обновляем каждый кадр призрак RoadGhost

        //toggle режим
        if (Input.GetKeyDown(toggleKey))
        {
            isPainting = !isPainting;
            if (!isPainting)
            {
                if (RoadGhost != null) RoadGhost.gameObject.SetActive(false);
            }

            if (isPainting)
            {
                runningRoadMode = true;
                Debug.Log("RoadBuildingMode running");
            }
            else
            {
                runningRoadMode = false;
                Debug.Log("RoadBuildingMode NOT running");
            }
        }

        if (!isPainting) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);
        Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

        //показываем RoadGhost под мышью
        if (RoadGhost != null)
        {
            RoadGhost.position = placePosition;
            RoadGhost.gameObject.SetActive(true);
        }

        bool shouldPaint = !requireHoldMouse || Input.GetMouseButton(0);

        if (shouldPaint)
        {
            //не ставим чаще, чем placeInterval
            if (Time.time - lastPlaceTime < placeInterval) return;

            //если brushSize > 1, делаем цикл по области
            List<Vector3Int> cellsToPlace = GetBrushCells(cellPosition, brushSize);

            //если нужно — суммарная проверка ресурсов перед тем, как поставить все клетки
            if (CanPlaceBatch(cellsToPlace))
            {
                foreach (var c in cellsToPlace)
                {
                    //избегаем повторной постановки на тот же cell подряд для оптимизации
                    if (c == lastPlacedCell && brushSize == 1)
                        continue;

                    PlaceRoadAt(c);
                    lastPlacedCell = c;
                    lastPlaceTime = Time.time;
                }
            }
            else
            {
                Debug.Log("Не хватает ресурсов на постойку дороги");
            }
        }
        //регулировка кисти колесом мыши
        int wheel = (int)Input.mouseScrollDelta.y;
        if (wheel != 0)
        {
            brushSize = Mathf.Clamp(brushSize + wheel, 1, 9); //допустим от 1 до 9
        }
    }

    private List<Vector3Int> GetBrushCells(Vector3Int center, int brush)
    {
        var list = new List<Vector3Int>();
        int radius = brush / 2; // brush 1 => radius 0; brush 3 => radius 1
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                list.Add(new Vector3Int(center.x + x, center.y + y, center.z));
        return list;
    }

    private bool CanPlaceBatch(List<Vector3Int> cells)
    {
        //проверяем, что хотя бы одна клетка не занята
        int toPlaceCount = 0;
        foreach (var c in cells)
            if (!RoadManager.Instance.IsRoadAt(c))
                toPlaceCount++;

        if (toPlaceCount == 0) return false;

        // Проверка ресурсов (если есть)
        if (costPerTile != null && costPerTile.Length > 0)
        {
            // создаём массив требуемых ресурсов = costPerTile * toPlaceCount
            var multiplied = new List<ResourceCost>();
            foreach (var rc in costPerTile)
                multiplied.Add(new ResourceCost { resourceType = rc.resourceType, amount = rc.amount * toPlaceCount });

            if (!ResourceStorage.Instance.CanAfford(multiplied.ToArray()))
                return false;
        }
        return true;
    }

    private void PlaceRoadAt(Vector3Int cell)
    {
        if (RoadManager.Instance.IsRoadAt(cell)) return; //уже дорога

        //списываем ресурсы за одну клетку (или делаем батч выше)
        if (costPerTile != null && costPerTile.Length > 0)
        {
            if (!ResourceStorage.Instance.CanAfford(costPerTile))
            {
                return;
            }
            ResourceStorage.Instance.DeductResources(costPerTile);
        }

        //ставим тайл
        roadTilemap.SetTile(cell, RoadPrefab);

        //добавляем в логическую структуру
        RoadManager.Instance.AddRoad(cell);

        //если используешь автотайлинг (RuleTile) — обнови соседние тайлы:
        UpdateNeighbors(cell);
    }

    private void UpdateNeighbors(Vector3Int cell)
    {
        Vector3Int[] dirs = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };
        foreach (var d in dirs)
            roadTilemap.RefreshTile(cell + d);
        roadTilemap.RefreshTile(cell);
    }

    //настройка призрак RoadGhost 
    private void RoadGhostRunning()
    {
        if (isPainting)
        {
            Builder.Instance.DestroyGhost();

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);
            Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

            if (RoadGhostInstance != null)
            {
                RoadGhostInstance.transform.position = placePosition;
                RoadGhostInstance.SetActive(true);
            }
        }
        else
        {
            if (RoadGhostInstance != null)
            {
                RoadGhostInstance.SetActive(false);
            }
        }
    }
}
