using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro; //для использования TextMeshPro

public class Builder : MonoBehaviour
{
    public static Builder Instance { get; private set; } //для ResourceProducer 

    [Header("Основные ссылки")]
    public Camera mainCamera; //камера
    public Tilemap buildTilemap; //сетка

    [Header("Данные зданий")]
    public BuildingData[] buildingDatas; //все здания с префабами и стоимостью

    [Header("Настройки строительства")]
    public GameObject ghostBuildingPrefab; //призрак здания

    //звуки
    public AudioClip buildSound; 
    public AudioClip destroySound;
    private AudioSource audioSource; //источник звука

    [Header("UI ресурсов")]
    public TextMeshProUGUI resourceText; //текст в панели сверху


    private GameObject ghostInstance; //текущий призрак здания на сцене
    private int currentBuildingIndex = 0; //индекс текущего выбранного здания
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); //клетки, на которых уже есть здания

    //ресурсы игрока
    private Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("Данные Bulldozer Mode")]
    public Button bulldozerButton;
    private Image bulldozerButtonImage;
    public bool bulldozerMode = false;
    public GameObject bulldozerGhostPrefab;
    private GameObject bulldozerGhostInstance;

    void Awake()
    {
        Instance = this; //для ResourceProducer 
    }

    void Start()
    {

        if (mainCamera == null)
            mainCamera = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();
        UpdateBulldozerButtonColor();

        //инициализируем ресурсы
        playerResources[ResourceType.Wood] = 100;
        playerResources[ResourceType.Stone] = 50;
        playerResources[ResourceType.Wool] = 0;

        SpawnGhost(buildingDatas[currentBuildingIndex].prefab); //показывают призрачную модель выбранного здания

        //создаём и загружаем BulldozerGhost
        GameObject ghost = Resources.Load<GameObject>("BulldozerGhost");
        if (ghost != null)
            bulldozerGhostInstance = Instantiate(ghost);

        UpdateResourceUI(); //обновляют интерфейс с ресурсами

    }

    void Update()
    {
        //если мышка наведена на UI, то игнорировать ввод
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //получаем мировые координаты мыши
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        //преобразуем в координаты клетки тайлмапа
        Vector3Int cellPosition = buildTilemap.WorldToCell(mouseWorldPos);

        //получаем центр клетки в мировых координатах
        Vector3 placePosition = buildTilemap.GetCellCenterWorld(cellPosition);

        //строительство зданий (если не включен режим бульдозера)
        if (!bulldozerMode)
        {
            //если не выбран Bulldozer, то прятать призрак bulldozerGhost
            if (bulldozerGhostInstance != null)
                bulldozerGhostInstance.SetActive(false);

            //перемещаем призрачное здание на текущую позицию
            ghostInstance.transform.position = placePosition;
            bool isOccupied = occupiedCells.Contains(cellPosition);

            if (isOccupied)
            {
                ghostInstance.SetActive(false); //выкл отображение, если клетка занята
            }
            else
            {
                ghostInstance.SetActive(true); //вкл, если это не так
            }

            //если нажата левая кнопка мыши
            if (Input.GetMouseButtonDown(0))
            {
                //проверяем, свободна ли клетка
                if (!occupiedCells.Contains(cellPosition))
                {
                    //получаем данные о текущем выбранном здании
                    BuildingData data = buildingDatas[currentBuildingIndex];

                    //проверяем, хватает ли ресурсов
                    if (CanAfford(data.cost))
                    {
                        //создаём здание
                        GameObject newBuilding = Instantiate(data.prefab, placePosition, Quaternion.identity);

                        //гарантированно добавляем BuildingInstance, если его нет
                        BuildingInstance bi = newBuilding.GetComponent<BuildingInstance>();
                        if (bi == null)
                        {
                            bi = newBuilding.AddComponent<BuildingInstance>();
                        }

                        bi.cost = data.cost;  //запоминаем стоимость

                        DeductResources(data.cost); //вычитаем ресурсы у игрока
                        occupiedCells.Add(cellPosition); //помечаем клетку как занятую
                        PlaySound(buildSound); //играем звук постройки
                        Debug.Log("Построено: " + data.prefab.name);
                    }
                    else
                    {
                        Debug.Log("Недостаточно ресурсов для постройки!");
                    }
                }
                else
                {
                    Debug.Log("Эта клетка уже занята!");
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
                //проверяем, есть ли коллайдер на месте клика
                Collider2D hitCollider = Physics2D.OverlapPoint(placePosition); 
                if (hitCollider != null)
                {
                    if (hitCollider.CompareTag("Building")) //если клик по зданию, то...
                    {
                        //получаем компонент BuildingInstance
                        BuildingInstance building = hitCollider.GetComponent<BuildingInstance>(); 
                        if (building != null && building.cost != null)
                        {
                            //возвращаем ресурсы игроку
                            AddResources(building.cost);
                            Debug.Log("Ресурсы возвращены");
                        }

                        //удаляем здание и очищаем клетку
                        Destroy(hitCollider.gameObject); 
                        occupiedCells.Remove(cellPosition);
                        PlaySound(destroySound);
                        Debug.Log("Здание продано и удалено");
                    }
                    //если клик по траве
                    else if (hitCollider.CompareTag("Grass"))
                    {
                        Destroy(hitCollider.gameObject);
                        occupiedCells.Remove(cellPosition); //на случай, если клетка была помечена занятой
                        PlaySound(destroySound);
                        Debug.Log("Трава удалена");
                    }
                }
            }
        }
    }

    //переключает режим "бульдозера"
    public void ToggleBulldozerMode()
    {
        bulldozerMode = !bulldozerMode; //вкл - выкл
        Debug.Log("Bulldozer mode: " + (bulldozerMode ? "ON" : "OFF"));
        UpdateBulldozerButtonColor(); //меняет цвет
    }

    //Обновляет цвет кнопки "бульдозера"
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
    private void SpawnGhost(GameObject buildingPrefab)
    {
        string ghostName = buildingPrefab.name + "Ghost"; //формируем имя файла-призрака
        //пытаемся загрузить префаб призрака из папки Resources
        GameObject ghostPrefab = Resources.Load<GameObject>(ghostName); 
        if (ghostPrefab == null)
        {
            ///если не найден призрачный префаб, используем оригинальный с прозрачностью
            Debug.LogWarning("Не найден призрак для " + buildingPrefab.name + ". Используем обычный префаб с прозрачностью");
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
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip); //воспроизводим звук
    }

    //проверяет, достаточно ли у игрока ресурсов для постройки!!!
    private bool CanAfford(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            //проверяем, есть ли указанный тип ресурса у игрока
            bool hasKey = playerResources.ContainsKey(item.resourceType);
            int value = hasKey ? playerResources[item.resourceType] : 0;

            Debug.Log($"Нужно: {item.amount} {item.resourceType}, есть: {value}");

            //если ресурса нет или его меньше, чем нужно — постройка невозможна
            if (!hasKey || value < item.amount) 
                return false;
        }
        return true; //все ресурсы в наличии и в достаточном количестве
    }

    //списывает ресурсы у игрока при строительстве здания
    private void DeductResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            playerResources[item.resourceType] -= item.amount; //уменьшаем значение указанного ресурса
        }
        UpdateResourceUI(); //обновляем интерфейс
    }

    //обновляет текстовое отображение ресурсов в интерфейсе
    private void UpdateResourceUI() 
    {
        if (resourceText == null) return;

        string result = "Ресурсы: ";
        List<string> parts = new List<string>();

        //собираем все пары "Тип: Кол-во"
        foreach (var kvp in playerResources)
        {
            parts.Add($"{kvp.Key}: {kvp.Value}");
        }

        result += string.Join(" | ", parts);

        resourceText.text = result;
    }

    //возвращает ресурсы игроку
    private void AddResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            //если ресурс не существует — инициализируем его
            if (!playerResources.ContainsKey(item.resourceType))
                playerResources[item.resourceType] = 0;

            //прибавляем количество ресурса
            playerResources[item.resourceType] += item.amount;

            Debug.Log($"Вернули {item.amount} {item.resourceType}");
        }
        UpdateResourceUI();
    }

    //добавляет один конкретный ресурс
    public void AddResource(ResourceType type, int amount)
    {
        if (!playerResources.ContainsKey(type))
            playerResources[type] = 0;

        playerResources[type] += amount;
        UpdateResourceUI();
    }

}