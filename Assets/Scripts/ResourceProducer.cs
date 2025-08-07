using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class ResourceProducer : MonoBehaviour //автоматическая добыча ресурсов на игровом объекте 
{
    public ResourceType resourceType; //тип ресурса
    public int amountPerCycle = 1; //сколько едениц ресурса производится за один цикл
    public float intervalSeconds = 1f; //интервал между циклами

    private float timer;

    private Builder builder;

    public CitizenClass requiredType = CitizenClass.Peasant; //класс требуемый для работа
    public int requiredPeople = 3; //кол-во нужное для работы здания
    private bool isActive = false; //флаг, работает ли здание сейчас

    private Tilemap tilemap;


    //метод при старте объекта
    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>(); //находит активный объект с компонентом builder на сцене

        //пытаемся назначить работников нужного класса и количества
        isActive = PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople);
        if (!isActive)
        {
            Debug.Log("Недостаточно людей, здание не работает");
            enabled = false; //отключаем Update, чтобы не тратить ресурсы
        }
    }

    //метод, который возвращает каждый кадр
    void Update()
    {
        timer += Time.deltaTime; //увеличивает таймер
        if (timer >= intervalSeconds) //если прошло достаточно времени, то обнулить таймер
        {
            timer = 0f;
            //если builder найден, добавляем ресурсы указанного типа
            ResourceStorage.Instance?.AddResource(resourceType, amountPerCycle);
        }

        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(transform.position);

        bool nearStorage = IsStorageNearby(cell);
        bool hasRoad = House.HasAdjacentRoad(cell); // уже должен быть

        if (hasRoad && nearStorage)
        {
            TryActivate();
        }
        else
        {
            Deactivate();
        }
    }

    //метод для попытки активации здания
    public void TryActivate()
    {
        //если здание уже активно, ничего не делаем
        if (isActive)
            return;

        //пытаемся назначить работников и активировать здание
        if (PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople))
        {
            isActive = true;
            Debug.Log($"{gameObject.name} начал работу: назначено {requiredPeople} {requiredType}");
            enabled = true; // если скрипт был отключен, включаем его
        }
    }

    //медот отключения здания
    public void Deactivate()
    {
        if (!isActive) return;

        PopulationManager.Instance.ReleasePeasants(requiredPeople);
        isActive = false;
        Debug.Log($"Здание {name} деактивировано, работники освобождены");
    }

    //вспомогательный метод, который проверит, есть ли дорога в конкретной клетке
    public bool IsRoadAtCell(Vector3Int cell)
    {
        Tilemap buildTilemap = Builder.Instance.buildTilemap;
        return buildTilemap.HasTile(cell);
    }

    //проверка расстояния от здания до склада по дороге
    public bool IsStorageNearby(Vector3Int startCell, int maxDistance = 20)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            //проверяем, есть ли склад в этой клетке
            if (IsStorageAtCell(current))
            {
                return true; // Склад найден
            }

            //прекращаем, если превышен лимит
            if (Vector3Int.Distance(current, startCell) > maxDistance)
                continue;

            //перебираем соседние клетки
            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;

                if (!visited.Contains(neighbor) && IsRoadAtCell(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false; //склад не найден в пределах
    }

    //стандартные направления 
    private static readonly Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
    };

    //проверка склада
    public bool IsStorageAtCell(Vector3Int cell)
    {
        Tilemap buildTilemap = Builder.Instance.buildTilemap;
        Vector3 worldPos = buildTilemap.GetCellCenterWorld(cell);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.1f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Building"))
            {
                if (col.GetComponent<Storage>() != null)
                    return true;
            }
        }
        return false;
    }




}
