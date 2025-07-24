using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;

public enum ResourceType //список возможных ресурсов
{
    Wood, //дерево
    Stone, //камень
    Wool, //шерсть
    Iron //железо
}


public class ResourceStorage : MonoBehaviour
{
    public static ResourceStorage Instance { get; private set; }

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>(); //ресурсы игрока

    [Header("UI ресурсов")]
    public TextMeshProUGUI resourceText; //текст в панели сверху

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        //инициализация ресурсов
        resources[ResourceType.Wood] = 999;
        resources[ResourceType.Wool] = 999;
        resources[ResourceType.Stone] = 999;
        resources[ResourceType.Iron] = 999;

        UpdateResourceUI(); //обновляют интерфейс с ресурсами
    }

    //проверяет достаточно ли ресурсов заданного типа у игрока 
    public bool HasEnough(ResourceType type, int amount)
    {
        return ResourceStorage.Instance.GetResourceAmount(type) >= amount;
    }

    //уменьшает количество заданного ресурса на нужное число
    public void Consume(ResourceType type, int amount)
    {
        ResourceStorage.Instance.SpendResource(type, amount);
    }

    public void Add(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;

        resources[type] += amount;
    }

    public int GetAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    public int GetResourceAmount(ResourceType type)
    {
        if (resources.TryGetValue(type, out int amount))
            return amount;
        return 0;
    }

    //возвращает ресурсы игроку
    public void AddResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            //если ресурс не существует — инициализируем его
            if (!resources.ContainsKey(item.resourceType))
                resources[item.resourceType] = 0;

            //прибавляем количество ресурса
            resources[item.resourceType] += item.amount;

            Debug.Log($"Вернули {item.amount} {item.resourceType}");
        }
        UpdateResourceUI();
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (GetResourceAmount(type) >= amount)
        {
            resources[type] -= amount;
            return true;
        }
        return false;
    }

    //обновляет текстовое отображение ресурсов в интерфейсе
    private void UpdateResourceUI()
    {
        if (resourceText == null) return;

        string result = "Ресурсы: ";
        List<string> parts = new List<string>();

        ResourceType[] displayOrder = new ResourceType[]
        {
            ResourceType.Wood,
            ResourceType.Stone,
            ResourceType.Wool,
            ResourceType.Iron
        };

        //собираем все пары "Тип: Кол-во"
        foreach (var type in displayOrder)
        {
            if (resources.TryGetValue(type, out int value))
            {
                parts.Add($"{type}: {value}");
            }
        }

        result += string.Join(" | ", parts);
        resourceText.text = result;
    }

    //проверяет, достаточно ли у игрока ресурсов для постройки
    public bool CanAfford(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            //проверяем, есть ли указанный тип ресурса у игрока
            bool hasKey = resources.ContainsKey(item.resourceType);
            int value = hasKey ? resources[item.resourceType] : 0;

            Debug.Log($"Нужно: {item.amount} {item.resourceType}, есть: {value}");

            //если ресурса нет или его меньше, чем нужно — постройка невозможна
            if (!hasKey || value < item.amount)
                return false;
        }
        return true; //все ресурсы в наличии и в достаточном количестве
    }

    //списывает ресурсы у игрока при строительстве здания
    public void DeductResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            resources[item.resourceType] -= item.amount; //уменьшаем значение указанного ресурса
        }
        UpdateResourceUI(); //обновляем интерфейс
    }


    //добавляет один конкретный ресурс
    public void AddResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;

        resources[type] += amount;
        UpdateResourceUI();
    }
}
