using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

    public GameObject ghostInstance; //текущий призрак здания на сцене
    public int currentBuildingIndex = 0; //индекс текущего выбранного здания
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); //клетки, на которых уже есть здания
    

    [Header("Данные Bulldozer Mode")]
    public Button bulldozerButton;
    private Image bulldozerButtonImage;
    public bool bulldozerMode = false;
    public GameObject bulldozerGhostPrefab;
    public GameObject bulldozerGhostInstance;

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

        SpawnGhost(buildingDatas[currentBuildingIndex].prefab); //показывают призрачную модель выбранного здания
    }

    void Update()
    {
        if (ShouldIgnoreInput()) return; //игнор ввода

        //преобразуем в координаты клетки тайлмапа
        Vector3Int cellPosition = GetMouseCellPosition();

        //получаем центр клетки в мировых координатах
        Vector3 placePosition = buildTilemap.GetCellCenterWorld(cellPosition);

        if (!CursorMode.Instance.CursorModeRun)
        {
            if (bulldozerMode)
                HandleBulldozerMode(cellPosition, placePosition);
            else
                HandleBuildMode(cellPosition, placePosition);
        }
    }

    //управление режимом строительства
    private void HandleBuildMode(Vector3Int cellPosition, Vector3 placePosition)
    {
        HandleBuildingSelection();
        HideBulldozerGhost();
        MoveGhostBuilding(placePosition);

        if (BoostingManager.Instance.isBoostingMode) return;

        ShowOrHideGhost(cellPosition);

        if (Input.GetMouseButtonDown(0))
            TryPlaceBuilding(cellPosition, placePosition);
    }

    //управление режимом бульдозера
    private void HandleBulldozerMode(Vector3Int cellPosition, Vector3 placePosition)
    {
        if (ghostInstance != null) ghostInstance.SetActive(false);
        if (bulldozerGhostInstance != null)
        {
            bulldozerGhostInstance.transform.position = placePosition;
            bulldozerGhostInstance.SetActive(true);
        }

        if (Input.GetMouseButtonDown(0)) TryDestroy(cellPosition, placePosition);
    }

    //попытка поставить объект на сцену
    private void TryPlaceBuilding(Vector3Int cellPosition, Vector3 placePosition)
    {
        if (BoostingManager.Instance.isBoostingMode && RoadPainter.Instance.isPainting) return;
        if (occupiedCells.Contains(cellPosition)) Debug.Log("Эта клетка уже занята!");
        if (RoadPainter.Instance.buildTilemap.GetTile(cellPosition) != null)
            RoadPainter.Instance.buildTilemap.SetTile(cellPosition, null);
        if (!House.HasAdjacentRoad(cellPosition))
        {
            Debug.Log("Невозможно построить: нет дороги рядом");
            return;
        }

        BuildingData data = buildingDatas[currentBuildingIndex];
        if (!ResourceStorage.Instance.CanAfford(data.cost))
        {
            Debug.Log("Недостаточно ресурсов для постройки!");
            return;
        }

        GameObject newBuilding = Instantiate(data.prefab, placePosition, Quaternion.identity);

        BuildingInstance bi = newBuilding.GetComponent<BuildingInstance>();
        if (bi == null) bi = newBuilding.AddComponent<BuildingInstance>();
        bi.cost = data.cost;

        if (newBuilding.TryGetComponent(out House house) && !house.PlaceHouse())
        {
            Destroy(newBuilding);
            return;
        }

        ResourceStorage.Instance.DeductResources(data.cost);
        occupiedCells.Add(cellPosition);
        PlaySound(buildSound);
    }

    //попытка уничтожить объект на сцене
    private void TryDestroy(Vector3Int cellPosition, Vector3 placePosition)
    {
        TileBase tile = buildTilemap.GetTile(cellPosition);
        if (tile != null && tile == roadTile)
        {
            RemoveRoad(cellPosition);
            return;
        }
        DestroyHouse(cellPosition, placePosition);
    }

    //удаление дорог (contains bugs)
    private void RemoveRoad(Vector3Int cellPosition)
    {
        Debug.Log($"Найдена дорога на {cellPosition} — удаляем тайл дороги.");

        PopulationManager.Instance.DeactivateAllResourceProducers(); //приостанавливает все добывающее здания
        buildTilemap.SetTile(cellPosition, null);  //удаляем тайл с tilemap

        if (RoadManager.Instance != null) RoadManager.Instance.RemoveRoad(cellPosition); //уведомляем менеджер дорог
        PlaySound(destroySound);
        RoadManager.Instance.CheckBuildingsRoadAccess(); //после удаления дороги проверяем отрезанные дома
        return;
    }

    //управление выбором здания
    private void HandleBuildingSelection()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider == null) return;

        House house = hit.collider.GetComponent<House>();
        if (house != null)
            boostingButton.SetTarget(house);
    }

    //уничтожение дома
    public void DestroyHouse(Vector3Int cellPosition, Vector3 placePosition)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(placePosition);
        if (hitCollider == null && !hitCollider.CompareTag("Building"))
            return;

        Vector3Int buildingCell = buildTilemap.WorldToCell(hitCollider.transform.position);
        bool needsProducerDeactivation = false;

        //возврат базовой стоимости
        if (hitCollider.TryGetComponent(out BuildingInstance building) && building.cost != null)
        {
            ResourceStorage.Instance.AddResources(building.cost);
            Debug.Log("Ресурсы возвращены");
        }

        //возврат за тип дома и удаление жителей
        if (hitCollider.TryGetComponent(out House house))
        {
            ResourceCost[] extraRefund = GetRefundForHouseClass(house.houseClass);
            if (extraRefund.Length > 0)
            {
                ResourceStorage.Instance.AddResources(extraRefund);
                Debug.Log($"Дополнительно возвращено: {string.Join(", ", extraRefund.Select(r => $"{r.amount} {r.resourceType}"))}");
            }

            PopulationManager.Instance.UnregisterHouse(house);
            house.RemoveAllCitizens();
            needsProducerDeactivation = true;
        }

        //если склад или источник комфорта
        if (hitCollider.GetComponent<Storage>() || hitCollider.GetComponent<ComfortSource>())
            needsProducerDeactivation = true;

        //удаление здания
        occupiedCells.Remove(buildingCell);
        Destroy(hitCollider.gameObject);
        PlaySound(destroySound);
        Debug.Log("Здание удалено");

        //отключение всех добывающих зданий
        if (needsProducerDeactivation)
            PopulationManager.Instance.DeactivateAllResourceProducers();
    }

    //таблица возврата ресурсов по классу дома
    private static ResourceCost[] GetRefundForHouseClass(HouseClass houseClass)
    {
        switch (houseClass)
        {
            case HouseClass.Worker:
                return new[]
                {
                new ResourceCost { resourceType = ResourceType.Stone, amount = 10 },
                new ResourceCost { resourceType = ResourceType.Wood, amount = 30 }
            };
            case HouseClass.Engineer:
                return new[]
                {
                new ResourceCost { resourceType = ResourceType.Iron, amount = 10 },
                new ResourceCost { resourceType = ResourceType.Stone, amount = 10 },
                new ResourceCost { resourceType = ResourceType.Wood, amount = 30 }
            };
            default:
                return Array.Empty<ResourceCost>();
        }
    }

    //тоже самое что DestroyHouse, но по объекту
    public void DestroySpecificBuilding(GameObject buildingGO)
    {
        if (buildingGO == null) return;

        Vector3Int buildingCell = buildTilemap.WorldToCell(buildingGO.transform.position);
        bool needsProducerDeactivation = false;

        //базовые ресурсы
        if (buildingGO.TryGetComponent(out BuildingInstance bi) && bi.cost != null)
        {
            ResourceStorage.Instance.AddResources(bi.cost);
            Debug.Log("Ресурсы возвращены");
        }

        //бонусы за класс дома
        if (buildingGO.TryGetComponent(out House house))
        {
            ResourceCost[] extraRefund = GetRefundForHouseClass(house.houseClass);
            if (extraRefund.Length > 0)
            {
                ResourceStorage.Instance.AddResources(extraRefund);
                Debug.Log($"Дополнительно возвращено: {string.Join(", ", extraRefund.Select(r => $"{r.amount} {r.resourceType}"))}");
            }

            PopulationManager.Instance.UnregisterHouse(house);
            house.RemoveAllCitizens();
            needsProducerDeactivation = true;
        }

        //если склад или источник комфорта
        if (buildingGO.GetComponent<Storage>() || buildingGO.GetComponent<ComfortSource>())
            needsProducerDeactivation = true;

        //удаление
        Collider2D col = buildingGO.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        occupiedCells.Remove(buildingCell);
        Destroy(buildingGO);
        PlaySound(destroySound);
        Debug.Log("Здание удалено (DestroySpecificBuilding)");

        if (needsProducerDeactivation)
            PopulationManager.Instance.DeactivateAllResourceProducers();
    }


    //перевод позиции мыши с экрана в мировые координаты Unity
    private Vector3Int GetMouseCellPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return buildTilemap.WorldToCell(mouseWorldPos);
    }

    //спрятать призрак режима бульдозера
    private void HideBulldozerGhost()
    {
        if (bulldozerGhostInstance != null)
            bulldozerGhostInstance.SetActive(false);
    }

    //перемещаем призрачное здание на текущую позицию
    private void MoveGhostBuilding(Vector3 placePosition)
    {
        if (ghostInstance != null)
        {
            ghostInstance.transform.position = placePosition;
        }
    }

    //показать или спрять призрак
    private void ShowOrHideGhost(Vector3Int cellPosition)
    {
        if (ghostInstance == null) return;

        bool isOccupied = occupiedCells.Contains(cellPosition);
        ghostInstance.SetActive(!isOccupied);
    }


    //переключает режим "бульдозера"
    public void ToggleBulldozerMode()
    {
        bulldozerMode = !bulldozerMode; //вкл - выкл
        Debug.Log("Bulldozer mode: " + (bulldozerMode ? "ON" : "OFF"));
        UpdateBulldozerButtonColor(); //меняет цвет

        if (RoadPainter.Instance.isPainting) RoadPainter.Instance.DisableRoadMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
        if (CursorMode.Instance.CursorModeRun) CursorMode.Instance.DisableCursorMode();

        if (bulldozerMode)
        {
            // Если призрака ещё нет — создаём
            if (bulldozerGhostInstance == null && bulldozerGhostPrefab != null)
            {
                bulldozerGhostInstance = Instantiate(bulldozerGhostPrefab);
                SetGhostTransparency(bulldozerGhostInstance); // чтобы он был полупрозрачным
                bulldozerGhostInstance.SetActive(false);
            }
        }
        else
        {
            HideBulldozerGhost();
        }

    }

    //обновляет цвет кнопки "бульдозера"
    public void UpdateBulldozerButtonColor()
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

    //выключение режима бульдозера
    public void DisableBulldozerMode()
    {
        if (bulldozerMode)
        {
            bulldozerMode = false;
            Debug.Log("BulldozerMode: " + (bulldozerMode ? "ON" : "OFF"));
            UpdateBulldozerButtonColor();
            if (bulldozerGhostInstance != null) bulldozerGhostInstance.SetActive(false);
        }
    }

    //игнор ввода
    private bool ShouldIgnoreInput()
    {
        //если мышка наведена на UI, то игнорировать ввод
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        //получаем мировые координаты мыши
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 ||
            Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
        {
            return true; // мышь вне экрана — ничего не делаем
        }

        return false;
    }
}