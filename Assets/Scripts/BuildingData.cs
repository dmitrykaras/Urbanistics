using UnityEngine;

[System.Serializable]
public class BuildingData //описание одного здания
{
    public GameObject prefab; //префаб здания
    public ResourceCost[] cost; //стоимость здания в ресурсах
}
