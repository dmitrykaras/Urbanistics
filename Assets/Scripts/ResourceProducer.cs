using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceProducer : MonoBehaviour
{
    public ResourceType resourceType; //��� �������
    public int amountPerCycle = 1; //������� ������ ������� ������������ �� ���� ����
    public float intervalSeconds = 1f; //�������� ����� �������

    private float timer;

    private Builder builder;

    public CitizenClass requiredType = CitizenClass.Peasant; //����� ��������� ��� ������
    public int requiredPeople = 3; //���-�� ������ ��� ������ ������
    public bool isActive = false; //����, �������� �� ������ ������

    private float conditionCheckTimer = 0f;
    public float conditionCheckInterval = 3.0f; //��� ����� ��������� ������� ������/������

    private bool flagMassage = false;

    public string buildingName;

    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>();
        isActive = false;
        timer = 0f;
        conditionCheckTimer = 0f;

        buildingName = gameObject.name.Replace("(Clone)", "");
    }

    void Update()
    {
        UpdateWork();
    }

    //����� ��� ����������
    public void ForceDeactivate() => Deactivate();

    public void UpdateWork()
    {
        conditionCheckTimer += Time.deltaTime;
        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(transform.position);

        if (!isActive)
        {
            if (conditionCheckTimer >= conditionCheckInterval)
            {
                conditionCheckTimer = 0f;

                bool canWork = HasRequiredConditions();

                if (canWork && !isActive)
                {
                    TryActivate();
                }
            }
            if (!flagMassage)
            {
                DebugMassage();
            }
        }
        else ResourceAdd();
    }

    //��������, ��� ������, ����� � ��������� ���� ����
    public bool HasRequiredConditions()
    {
        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(transform.position);
        return IsStorageNearby(cell, 20)
            && PopulationManager.Instance.CanAssignWorkers(requiredType, requiredPeople);
    }

    //���������� �������� (�������� ������)
    private void ResourceAdd()
    {
        timer += Time.deltaTime;
        if (timer >= intervalSeconds)
        {
            timer -= intervalSeconds; //����� ��������, ����� �� ������ ������� �������
            ResourceStorage.Instance?.AddResource(resourceType, amountPerCycle);
        }
    }

    //����� ��� ������� ��������� ������
    public void TryActivate()
    {
        if (isActive) return;

        if (PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople))
        {
            isActive = true;
            Debug.Log($"{gameObject.name} ����� ������: ��������� {requiredPeople} {requiredType}");
        }
    }

    //����� ���������� ������
    public void Deactivate()
    {
        if (!isActive) return;

        PopulationManager.Instance.ReleaseWorkers(requiredType, requiredPeople);
        isActive = false;
        Debug.Log($"������ {name} ��������������, ��������� �����������");
    }

    //��������������� �����, ������� ��������, ���� �� ������ � ���������� ������
    public bool IsRoadAtCell(Vector3Int cell)
    {
        return RoadManager.Instance.IsRoadAt(cell);
    }

    //�������� ���������� �� ������ �� ������ �� ������
    public bool IsStorageNearby(Vector3Int startCell, int maxDistance = 20)
    {
        Queue<(Vector3Int cell, int dist)> queue = new Queue<(Vector3Int, int)>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue((startCell, 0));
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            if (IsStorageAtCell(current))
                return true;

            if (dist >= maxDistance)
                continue;

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;

                if (!visited.Contains(neighbor) && IsRoadAtCell(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
        return false;
    }

    //����������� ����������� 
    private static readonly Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
    };

    //�������� ������
    public bool IsStorageAtCell(Vector3Int cell)
    {
        Tilemap buildTilemap = Builder.Instance.buildTilemap;
        Vector3 worldPos = buildTilemap.GetCellCenterWorld(cell);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 1.0f);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Building") && col.GetComponent<Storage>() != null)
                return true;
        }
        return false;
    }

    //���������, ������� ����� ���� ��� ��������� �� �����
    private void DebugMassage()
    {
        flagMassage = true;
        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(transform.position);
        if (!IsStorageNearby(cell, 20)) Debug.Log("������ �� ��������. ������ ����� ���");
        if (!HasRequiredConditions()) Debug.Log("������ �� ��������. �� ������� ��������� ���");
    }
}
