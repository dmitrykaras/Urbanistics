using UnityEngine;

[System.Serializable] //отображение в инспекторе
public class ResourceCost //сколько какого ресурса требует здание
{
    public ResourceType resourceType; //тип ресурса 
    public int amount; //сколько нужно
}
