using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPainter : MonoBehaviour
{
    [Header("ќсновные настройки")]
    public Tilemap roadTilemap; //Tilemap, на которой рисуем дороги
    public TileBase roadTile; //сам тайл дороги (можно RuleTile дл€ авто-тайлинга)
    public Transform ghost; //объект-призрак (спрайт) Ч optional
    public Camera mainCamera;

    [Header("Ќастройки")]
    public KeyCode toggleKey = KeyCode.R;
    public int brushSize = 1; //1 = одна клетка, 2 = 3x3 и т.д. (несколько реализаций)
    public float placeInterval = 0.02f; //минимальный интервал между постановками при drag
    public bool requireHoldMouse = true; //true Ч рисуем только при зажатой Ћ ћ

    //ресурсы на тайл
    public ResourceCost[] costPerTile; // если у теб€ есть ResourceCost struct

    private bool isPainting = false; //включение и выключение режима строительства дорог
    private float lastPlaceTime = 0f;
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, 0);

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (ghost != null) ghost.gameObject.SetActive(false);
    }

    private void Update()
    {
        //toggle режим
        if (Input.GetKeyDown(toggleKey))
        {
            isPainting = !isPainting;
            if (!isPainting)
            {
                if (ghost != null) ghost.gameObject.SetActive(false);
            }
        }

        if (!isPainting) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(mouseWorld);
        Vector3 cellCenterWorld = Builder.Instance.buildTilemap.GetCellCenterWorld(cell);

        //показываем ghost под мышью
        if (ghost != null)
        {
            ghost.position = cellCenterWorld;
            ghost.gameObject.SetActive(true);
        }

        bool shouldPaint = !requireHoldMouse || Input.GetMouseButton(0);

        if (shouldPaint)
        {
            //не ставим чаще, чем placeInterval
            if (Time.time - lastPlaceTime < placeInterval) return;

            //если brushSize > 1, делаем цикл по области
            List<Vector3Int> cellsToPlace = GetBrushCells(cell, brushSize);

            //если нужно Ч суммарна€ проверка ресурсов перед тем, как поставить все клетки
            if (CanPlaceBatch(cellsToPlace))
            {
                foreach (var c in cellsToPlace)
                {
                    //избегаем повторной постановки на тот же cell подр€д дл€ оптимизации
                    if (c == lastPlacedCell && brushSize == 1)
                        continue;

                    PlaceRoadAt(c);
                    lastPlacedCell = c;
                    lastPlaceTime = Time.time;
                }
            }
            else
            {
                //можно проиграть звук/показать UI "нет ресурсов"
            }
        }

        //выход правой кнопкой мыши
        if (Input.GetMouseButtonDown(1))
        {
            isPainting = false;
            if (ghost != null) ghost.gameObject.SetActive(false);
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
        //провер€ем, что хот€ бы одна клетка не зан€та
        int toPlaceCount = 0;
        foreach (var c in cells)
            if (!RoadManager.Instance.IsRoadAt(c))
                toPlaceCount++;

        if (toPlaceCount == 0) return false;

        // ѕроверка ресурсов (если есть)
        if (costPerTile != null && costPerTile.Length > 0)
        {
            // создаЄм массив требуемых ресурсов = costPerTile * toPlaceCount
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
        roadTilemap.SetTile(cell, roadTile);

        //добавл€ем в логическую структуру
        RoadManager.Instance.AddRoad(cell);

        //если используешь автотайлинг (RuleTile) Ч обнови соседние тайлы:
        UpdateNeighbors(cell);
    }

    private void UpdateNeighbors(Vector3Int cell)
    {
        Vector3Int[] dirs = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };
        foreach (var d in dirs)
            roadTilemap.RefreshTile(cell + d);
        roadTilemap.RefreshTile(cell);
    }
}
