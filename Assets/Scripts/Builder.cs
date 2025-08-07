using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    public static Builder Instance { get; private set; } //для ResourceProducer 

    [Header("Основные ссылки")]
    public Camera mainCamera; //камера
    public Tilemap buildTilemap; //сетка

    [Header("Данные зданий")]
    public BuildingData[] buildingDatas; //все здания с префабами и стоимостью

    [Header("Настройки строительства")]
    //звуки
    public AudioClip buildSound; 
    public AudioClip destroySound;
    private AudioSource audioSource; //источник звука

    private GameObject ghostInstance; //текущий призрак здания на сцене
    public int currentBuildingIndex = 0; //индекс текущего выбранного здания
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); //клетки, на которых уже есть здания
    

    [Header("Данные Bulldozer Mode")]
    public Button bulldozerButton;
    private Image bulldozerButtonImage;
    private bool bulldozerMode = false;
    public GameObject bulldozerGhostPrefab;
    private GameObject bulldozerGhostInstance;

    [Header("Улучшения зданий")]
    public BoostingButton boostingButton; //ссылка на кнопку boosting

    [Header("Строительство дорог")]
    public TileBase roadTile; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();
        UpdateBulldozerButtonColor();

        SpawnGhost(buildingDatas[currentBuildingIndex].prefab); //показывают призрачную модель выбранного здания

        //создаём и загружаем BulldozerGhost
        GameObject ghost = Resources.Load<GameObject>("BulldozerGhost");
        if (ghost != null)
            bulldozerGhostInstance = Instantiate(ghost);
        
    }

    void Update()
    {
        //если мышка наведена на UI, то игнорировать ввод
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //получаем мировые координаты мыши
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 ||
            Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
        {
            return; // мышь вне экрана — ничего не делаем
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //преобразуем в координаты клетки тайлмапа
        Vector3Int cellPosition = buildTilemap.WorldToCell(mouseWorldPos);

        //получаем центр клетки в мировых координатах
        Vector3 placePosition = buildTilemap.GetCellCenterWorld(cellPosition);

        //строительство зданий (если не включен режим бульдозера)
        if (!bulldozerMode)
        {

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    House house = hit.collider.GetComponent<House>();
                    if (house != null)
                    {
                        // передаём выбранный дом кнопке улучшения
                        boostingButton.SetTarget(house);
                    }
                }
            }

            //если не выбран Bulldozer, то прятать призрак bulldozerGhost
            if (bulldozerGhostInstance != null)
                bulldozerGhostInstance.SetActive(false);

            //перемещаем призрачное здание на текущую позицию
            if (ghostInstance != null)
            {
                ghostInstance.transform.position = placePosition;
            }
            bool isOccupied = occupiedCells.Contains(cellPosition);
            if (!BoostingManager.Instance.runningBoostingMode)
            {
                if (isOccupied)
                {
                    if (ghostInstance != null)
                        ghostInstance.SetActive(false); //выкл отображение, если клетка занята
                }
                else
                {
                    if (ghostInstance != null)
                        ghostInstance.SetActive(true); //вкл, если это не так
                }
            }

            //если нажата левая кнопка мыши
            if (Input.GetMouseButtonDown(0))
            {
                if (!BoostingManager.Instance.runningBoostingMode && !RoadPainter.Instance.runningRoadMode)
                {
                    if (!occupiedCells.Contains(cellPosition))
                    {
                        // Проверка дороги
                        if (House.HasAdjacentRoad(cellPosition))
                        {
                            BuildingData data = buildingDatas[currentBuildingIndex];

                            if (ResourceStorage.Instance.CanAfford(data.cost))
                            {
                                GameObject newBuilding = Instantiate(data.prefab, placePosition, Quaternion.identity);

                                BuildingInstance bi = newBuilding.GetComponent<BuildingInstance>();
                                if (bi == null) bi = newBuilding.AddComponent<BuildingInstance>();
                                bi.cost = data.cost;

                                bool placementOk = true;
                                if (newBuilding.TryGetComponent<House>(out House houseComponent))
                                {
                                    placementOk = houseComponent.PlaceHouse();
                                }

                                if (!placementOk)
                                {
                                    Destroy(newBuilding);
                                }
                                else
                                {
                                    ResourceStorage.Instance.DeductResources(data.cost);
                                    occupiedCells.Add(cellPosition);
                                    PlaySound(buildSound);
                                    Debug.Log("Построено: " + data.prefab.name);
                                }
                            }
                            else
                            {
                                Debug.Log("Недостаточно ресурсов для постройки!");
                            }
                        }
                        else
                        {
                            Debug.Log("Невозможно построить: нет дороги рядом.");
                        }
                    }
                    else
                    {
                        Debug.Log("Эта клетка уже занята!");
                    }
                }
            }
        }
        //режим бульдозера
        else
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);

            //если bulldozerGhostInstance существет, то перемещаем призрак под курсором и отображем его
            if (bulldozerGhostInstance != null)
            {
                bulldozerGhostInstance.transform.position = placePosition;
                bulldozerGhostInstance.SetActive(true);
            }

            if (Input.GetMouseButtonDown(0)) //при нажатии на ЛКМ
            {
                TileBase tile = buildTilemap.GetTile(cellPosition);
                if (tile != null && roadTile != null && tile == roadTile)
                {
                    Debug.Log($"Найдена дорога на {cellPosition} — удаляем тайл дороги.");

                    //удаляем тайл с tilemap
                    buildTilemap.SetTile(cellPosition, null);

                    //уведомляем менеджер дорог (если у тебя есть RoadManager для данных)
                    if (RoadManager.Instance != null)
                    {
                        RoadManager.Instance.RemoveRoad(cellPosition);
                    }

                    PlaySound(destroySound);

                    //после удаления дороги проверяем отрезанные дома
                    RoadManager.Instance.CheckBuildingsRoadAccess();

                    return;
                }

                //если дорога не удалена, только тогда пытаемся удалить здание
                DestroyHouse();

            }
        }
    }

    public void DestroyHouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = buildTilemap.WorldToCell(mouseWorldPos);
        Vector3 placePosition = buildTilemap.GetCellCenterWorld(cellPosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(placePosition);
        if (hitCollider != null)
        { 
            if (hitCollider.CompareTag("Building")) //если клик по зданию, то...
            {
                // 1. Возвращаем ресурсы, если есть
                BuildingInstance building = hitCollider.GetComponent<BuildingInstance>();
                if (building != null && building.cost != null)
                {
                    ResourceStorage.Instance.AddResources(building.cost);
                    Debug.Log("Ресурсы возвращены");
                }

                // 2. Если это дом — удалим жителей
                House house = hitCollider.GetComponent<House>();
                if (house != null)
                {
                    house.RemoveAllCitizens(); // удаление жителей ДО уничтожения дома
                }

                // 3. Удаляем здание
                Destroy(hitCollider.gameObject);
                occupiedCells.Remove(cellPosition);
                PlaySound(destroySound);
                Debug.Log("Здание удалено");
            }
        }
    }

    // удаление по GameObject
    public void DestroySpecificBuilding(GameObject buildingGO)
    {
        if (buildingGO == null) return;

        //определяем cell по позиции самого здания
        Vector3Int buildingCell = buildTilemap.WorldToCell(buildingGO.transform.position);

        //вернуть ресурсы
        BuildingInstance bi = buildingGO.GetComponent<BuildingInstance>();
        if (bi != null && bi.cost != null)
        {
            ResourceStorage.Instance.AddResources(bi.cost);
            Debug.Log("Ресурсы возвращены");
        }

        //если дом — удалить жителей
        House house = buildingGO.GetComponent<House>();
        if (house != null)
        {
            house.RemoveAllCitizens();
        }

        //отключаем коллайдер, удаляем из occupiedCells и уничтожаем объект
        Collider2D col = buildingGO.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (occupiedCells.Remove(buildingCell))
            Debug.Log($"Клетка {buildingCell} освобождена из occupiedCells");
        else
            Debug.LogWarning($"Попытка удалить клетку {buildingCell} из occupiedCells — её там не было");

        Destroy(buildingGO);
        PlaySound(destroySound);
        Debug.Log("Здание удалено (DestroySpecificBuilding)");
    }


    //переключает режим "бульдозера"
    public void ToggleBulldozerMode()
    {
        if (RoadPainter.Instance.isPainting)
        {
            RoadPainter.Instance.isPainting = false;
            if (!RoadPainter.Instance.isPainting)
                Debug.Log("RoadBuilderMode: OFF");
        }

        bulldozerMode = !bulldozerMode; //вкл - выкл
        Debug.Log("Bulldozer mode: " + (bulldozerMode ? "ON" : "OFF"));
        UpdateBulldozerButtonColor(); //меняет цвет
    }

    //обновляет цвет кнопки "бульдозера"
    private void UpdateBulldozerButtonColor()
    {
        //если ссылка на компонент Image ещё не установлена — ищем её на кнопке
        if (bulldozerButtonImage == null) 
            bulldozerButtonImage = bulldozerButton.GetComponent<Image>();
        //задаём цвет кнопки в зависимости от состояния
        bulldozerButtonImage.color = bulldozerMode ? new Color(1f, 0.4f, 0.4f, 1f) : Color.white;
    }

    //вызывается при выборе здания 
    public void SelectBuilding(int index)
    {
        //проверка: индекс не выходит за границы массива
        if (index < 0 || index >= buildingDatas.Length) 
        {
            Debug.LogWarning("Неверный индекс здания");
            return;
        }

        currentBuildingIndex = index; //запоминаем выбранное здание

        if (ghostInstance != null) 
            Destroy(ghostInstance); //удаляем предыдущий призрачный объект, если был

        SpawnGhost(buildingDatas[currentBuildingIndex].prefab); //создаём новый призрак выбранного здания

        Debug.Log("Выбрано здание: " + buildingDatas[currentBuildingIndex].prefab.name);
    }

    //создаёт "призрачную" версию здания
    public void SpawnGhost(GameObject buildingPrefab)
    {
        string ghostName = buildingPrefab.name + "Ghost"; //формируем имя файла-призрака
        //пытаемся загрузить префаб призрака из папки Resources
        GameObject ghostPrefab = Resources.Load<GameObject>(ghostName); 
        if (ghostPrefab == null)
        {
            //если не найден призрачный префаб, используем оригинальный с прозрачностью
            ghostInstance = Instantiate(buildingPrefab);
            SetGhostTransparency(ghostInstance);
        }
        else
        {
            ghostInstance = Instantiate(ghostPrefab); //если призрачный префаб найден — используем его
        }

        //выключает добычу ресурсов у призраков зданий
        ResourceProducer producer = ghostInstance.GetComponent<ResourceProducer>();
        if (producer != null) 
            producer.enabled = false; //выключение добычи

    }

    //делает объект визуально полупрозрачным и отключает коллайдеры
    private void SetGhostTransparency(GameObject obj)
    {
        //находим все спрайты в объекте
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var rend in renderers)
        {
            Color c = rend.color; 
            c.a = 0.4f; //устанавливаем прозрачность
            rend.color = c; //применяем
        }

        //отключаем все коллайдеры — чтобы призрак не мешал размещению/кликам
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    //воспроизводит однократный звук
    public void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.clip = clip;
        audioSource.Stop();  // остановить предыдущий звук
        audioSource.Play();  // запустить заново
    }

    //убирает все остальны призраки кроме одного
    public void DestroyGhost()
    {
        if (ghostInstance != null)
            ghostInstance.SetActive(false);
    }

}