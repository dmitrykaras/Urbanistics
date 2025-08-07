using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance { get; private set; }

    //хранилище позиций дорог
    public HashSet<Vector3Int> roadPositions = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public void AddRoad(Vector3Int position)
    {
        roadPositions.Add(position);
    }

    public void RemoveRoad(Vector3Int position)
    {
        roadPositions.Remove(position);
    }

    public bool IsRoadAt(Vector3Int position)
    {
        return roadPositions.Contains(position);
    }

    //проверка отрезанных зданий после удаления дороги
    public void CheckDisconnectedBuildings()
    {
        House[] allHouses = Object.FindObjectsByType<House>(FindObjectsSortMode.None);

        foreach (var house in allHouses)
        {
            Vector3Int cellPos = Builder.Instance.buildTilemap.WorldToCell(house.transform.position);
            if (!House.HasAdjacentRoad(cellPos))
            {
                Debug.Log("Дом потерял доступ к дороге и будет разрушен.");
                Builder.Instance.DestroySpecificBuilding(house.gameObject);
            }
        }
    }
}
