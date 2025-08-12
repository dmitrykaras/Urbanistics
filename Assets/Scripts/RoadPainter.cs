using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class RoadPainter : MonoBehaviour
{
    public static RoadPainter Instance { get; private set; }

    [Header("Основные настройки")]
    public Tilemap buildTilemap; //Tilemap, на которой рисуем дороги
    public TileBase RoadTile; //сам тайл дороги (можно RuleTile для авто-тайлинга)
    public Transform RoadGhost; //объект-призрак (спрайт) — optional
    public Camera mainCamera;

    [Header("Настройки")]
    public KeyCode toggleKey = KeyCode.R;
    public int brushSize = 1; //1 = одна клетка, 2 = 3x3 и т.д. (несколько реализаций)
    public float placeInterval = 0.02f; //минимальный интервал между постановками при drag
    public bool requireHoldMouse = true; //true — рисуем только при зажатой ЛКМ

    [Header("Стоимость")]
    //ресурсы на тайл
    public ResourceCost[] costPerTile; // если у тебя есть ResourceCost struct

    [Header("Остальные настройки")]
    public bool isPainting = false; //включение и выключение режима строительства дорог
    private float lastPlaceTime = 0f;

    public GameObject RoadGhostInstance;

    [Header("Звуки")]
    public AudioClip sand;

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

    void Update()
    {
        //если мышка наведена на UI, то игнорировать ввод
        if (EventSystem.current.IsPointerOverGameObject()) return;

        RoadGhostRunning(); //обновляем каждый кадр призрак RoadGhost

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
            if (Time.time - lastPlaceTime < placeInterval) return;

            if (!RoadManager.Instance.IsRoadAt(cellPosition))
            {
                if (ResourceStorage.Instance.CanAfford(costPerTile))
                {
                    ResourceStorage.Instance.DeductResources(costPerTile);
                    PlaceRoadAt(cellPosition);
                    Builder.Instance.PlaySound(sand);
                    lastPlaceTime = Time.time;
                }
                else
                {
                    Debug.Log("Не хватает ресурсов на постройку дороги");
                }
            }
        }

    }

    public void ToggleRoadPainter()
    {
        isPainting = !isPainting;
        Debug.Log("RoadBuilderMode: " + (isPainting ? "ON" : "OFF"));
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
    }

    //ставим дорогу
    private void PlaceRoadAt(Vector3Int cell)
    {
        if (RoadManager.Instance.IsRoadAt(cell)) return; //уже дорога

        //списываем ресурсы за одну клетку
        if (costPerTile != null && costPerTile.Length > 0)
        {
            if (!ResourceStorage.Instance.CanAfford(costPerTile))
            {
                return;
            }
            ResourceStorage.Instance.DeductResources(costPerTile);
        }

        //ставим тайл
        buildTilemap.SetTile(cell, RoadTile);

        //добавляем в логическую структуру
        RoadManager.Instance.AddRoad(cell);

        Builder.Instance.PlaySound(sand);
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

    //выключение режима строительства дорог
    public void DisableRoadMode()
    {
        if (isPainting)
        {
            isPainting = false;
            Debug.Log("RoadBuilderMode: " + (isPainting ? "ON" : "OFF"));
            RoadGhostInstance.SetActive(false);
        }
    }
}
