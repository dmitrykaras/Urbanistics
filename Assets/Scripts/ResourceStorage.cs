using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;

public enum ResourceType //������ ��������� ��������
{
    Wood, //������
    Stone, //������
    Wool, //������
    Iron //������
}


public class ResourceStorage : MonoBehaviour
{
    public static ResourceStorage Instance { get; private set; }

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>(); //������� ������

    [Header("UI ��������")]
    public TextMeshProUGUI resourceText; //����� � ������ ������

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        //������������� ��������
        resources[ResourceType.Wood] = 999;
        resources[ResourceType.Wool] = 999;
        resources[ResourceType.Stone] = 999;
        resources[ResourceType.Iron] = 999;

        UpdateResourceUI(); //��������� ��������� � ���������
    }

    //��������� ���������� �� �������� ��������� ���� � ������ 
    public bool HasEnough(ResourceType type, int amount)
    {
        return ResourceStorage.Instance.GetResourceAmount(type) >= amount;
    }

    //��������� ���������� ��������� ������� �� ������ �����
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

    //���������� ������� ������
    public void AddResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            //���� ������ �� ���������� � �������������� ���
            if (!resources.ContainsKey(item.resourceType))
                resources[item.resourceType] = 0;

            //���������� ���������� �������
            resources[item.resourceType] += item.amount;

            Debug.Log($"������� {item.amount} {item.resourceType}");
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

    //��������� ��������� ����������� �������� � ����������
    private void UpdateResourceUI()
    {
        if (resourceText == null) return;

        string result = "�������: ";
        List<string> parts = new List<string>();

        ResourceType[] displayOrder = new ResourceType[]
        {
            ResourceType.Wood,
            ResourceType.Stone,
            ResourceType.Wool,
            ResourceType.Iron
        };

        //�������� ��� ���� "���: ���-��"
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

    //���������, ���������� �� � ������ �������� ��� ���������
    public bool CanAfford(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            //���������, ���� �� ��������� ��� ������� � ������
            bool hasKey = resources.ContainsKey(item.resourceType);
            int value = hasKey ? resources[item.resourceType] : 0;

            Debug.Log($"�����: {item.amount} {item.resourceType}, ����: {value}");

            //���� ������� ��� ��� ��� ������, ��� ����� � ��������� ����������
            if (!hasKey || value < item.amount)
                return false;
        }
        return true; //��� ������� � ������� � � ����������� ����������
    }

    //��������� ������� � ������ ��� ������������� ������
    public void DeductResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            resources[item.resourceType] -= item.amount; //��������� �������� ���������� �������
        }
        UpdateResourceUI(); //��������� ���������
    }


    //��������� ���� ���������� ������
    public void AddResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;

        resources[type] += amount;
        UpdateResourceUI();
    }
}
