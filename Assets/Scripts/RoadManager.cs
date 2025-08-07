using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance { get; private set; }

    //��������� ������� �����
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

    //�������� ���������� ������ ����� �������� ������
    public void CheckBuildingsRoadAccess()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
        foreach (GameObject building in buildings)
        {
            Vector3Int cellPos = Builder.Instance.buildTilemap.WorldToCell(building.transform.position);
            if (!House.HasAdjacentRoad(cellPos))
            {
                Debug.Log("��� ������� ������ � ������ � ����� ��������.");
                Builder.Instance.DestroySpecificBuilding(building.gameObject);
            }
        }
    }
}
