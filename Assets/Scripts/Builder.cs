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

    private GameObject ghostInstance; //текущий призрак здания на сцене
    private int currentBuildingIndex = 0; //индекс текущего выбранного здания
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); //клетки, на которых уже есть здания
    

    [Header("Данные Bulldozer Mode")]
    public Button bulldozerButton;
    private Image bulldozerButtonImage;
    public bool bulldozerMode = false;
    public GameObject bulldozerGhostPrefab;
    private GameObject bulldozerGhostInstance;

    public BoostingButton boostingButton; //ссылка на кнопку boosting


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
                        Debug.Log("Дом выбран через Raycast");
                    }
                }
            }

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
                    if (ResourceStorage.Instance.CanAfford(data.cost))
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

                        //если это дом — вызываем PlaceHouse, чтобы заселение было корректным
                        if (newBuilding.TryGetComponent<House>(out House houseComponent))
                        {
                            houseComponent.PlaceHouse();
                        }


                        ResourceStorage.Instance.DeductResources(data.cost); //вычитаем ресурсы у игрока
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
}