using System.Collections.Generic;
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

    public void AddRoad(Vector3Int cellPosition)
    {
        roadPositions.Add(cellPosition);
    }

    public void RemoveRoad(Vector3Int cellPosition)
    {
        roadPositions.Remove(cellPosition);
        Builder.Instance.occupiedCells.Remove(cellPosition);
    }

    public bool IsRoadAt(Vector3Int cellPosition)
    {
        return roadPositions.Contains(cellPosition);
    }

    //�������� ���������� ������ ����� �������� ������
    public void CheckBuildingsRoadAccess()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        foreach (GameObject building in buildings)
        {
            if (building.layer == LayerMask.NameToLayer("RoadLayer"))
                continue;

            Vector3Int cellPos = Builder.Instance.buildTilemap.WorldToCell(building.transform.position);
            if (!House.HasAdjacentRoad(cellPos))
            {
                Debug.Log("��� ������� ������ � ������ � ����� ��������");
                Builder.Instance.DestroySpecificBuilding(building.gameObject);
            }
        }

    }
}
