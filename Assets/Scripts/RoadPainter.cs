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

    [Header("Остальные настройки")]
    public bool isPainting = false; //включение и выключение режима строительства дорог

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
        Vector3 mouseWorldPos = Builder.Instance.GetMouseCellPosition();
        Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);
        Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

        //если мышка наведена на UI, то игнорировать ввод
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRoadPainter();
        }

        RoadGhostRunning(cellPosition, placePosition); //обновляем каждый кадр призрак RoadGhost

        if (!isPainting) return;

        //показываем RoadGhost под мышью
        if (RoadGhost != null)
        {
            RoadGhost.position = placePosition;
            RoadGhost.gameObject.SetActive(true);
        }

        if (Input.GetMouseButtonDown(0) && (!RoadManager.Instance.IsRoadAt(cellPosition)))
        {
            PlaceRoadAt(cellPosition);
        }

    }

    public void ToggleRoadPainter()
    {
        isPainting = !isPainting;
        Debug.Log("RoadBuilderMode: " + (isPainting ? "ON" : "OFF"));
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
        if (CursorMode.Instance.CursorModeRun) CursorMode.Instance.DisableCursorMode();
    }

    //ставим дорогу
    private void PlaceRoadAt(Vector3Int cellPosition)
    {
        if (!Builder.Instance.occupiedCells.Add(cellPosition))
        {
            Debug.Log("Невозможно поставить дорогу: клетка уже занята!");
            return;
        }

        buildTilemap.SetTile(cellPosition, RoadTile); //ставим тайл

        RoadManager.Instance.AddRoad(cellPosition); //добавляем в логическую структуру

        Builder.Instance.occupiedCells.Add(cellPosition); //занимаем клетку дорогой

        Builder.Instance.PlaySound(sand); 
    }

    //настройка призрак RoadGhost 
    private void RoadGhostRunning(Vector3Int cellPosition, Vector3 placePosition)
    {
        if (isPainting)
        {
            Builder.Instance.DisablingGhost();

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
