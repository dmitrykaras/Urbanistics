using UnityEngine;
using System.Collections.Generic;


public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private Dictionary<ResourceType, int> resources = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool HasEnough(ResourceType type, int amount)
    {
        return resources.TryGetValue(type, out int value) && value >= amount;
    }

    public void Consume(ResourceType type, int amount)
    {
        if (HasEnough(type, amount))
            resources[type] -= amount;
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;
        resources[type] += amount;
    }
}
