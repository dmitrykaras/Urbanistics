using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class ResourceProducer : MonoBehaviour //�������������� ������ �������� �� ������� ������� 
{
    public ResourceType resourceType; //��� �������
    public int amountPerCycle = 1; //������� ������ ������� ������������ �� ���� ����
    public float intervalSeconds = 1f; //�������� ����� �������

    private float timer;

    private Builder builder;

    public CitizenClass requiredType = CitizenClass.Peasant; //����� ��������� ��� ������
    public int requiredPeople = 3; //���-�� ������ ��� ������ ������
    private bool isActive = false; //����, �������� �� ������ ������

    private Tilemap tilemap;


    //����� ��� ������ �������
    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>(); //������� �������� ������ � ����������� builder �� �����

        //�������� ��������� ���������� ������� ������ � ����������
        isActive = PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople);
        if (!isActive)
        {
            Debug.Log("������������ �����, ������ �� ��������");
            enabled = false; //��������� Update, ����� �� ������� �������
        }
    }

    //�����, ������� ���������� ������ ����
    void Update()
    {
        timer += Time.deltaTime; //����������� ������
        if (timer >= intervalSeconds) //���� ������ ���������� �������, �� �������� ������
        {
            timer = 0f;
            //���� builder ������, ��������� ������� ���������� ����
            ResourceStorage.Instance?.AddResource(resourceType, amountPerCycle);
        }

        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(transform.position);

        bool nearStorage = IsStorageNearby(cell);
        bool hasRoad = House.HasAdjacentRoad(cell); // ��� ������ ����

        if (hasRoad && nearStorage)
        {
            TryActivate();
        }
        else
        {
            Deactivate();
        }
    }

    //����� ��� ������� ��������� ������
    public void TryActivate()
    {
        //���� ������ ��� �������, ������ �� ������
        if (isActive)
            return;

        //�������� ��������� ���������� � ������������ ������
        if (PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople))
        {
            isActive = true;
            Debug.Log($"{gameObject.name} ����� ������: ��������� {requiredPeople} {requiredType}");
            enabled = true; // ���� ������ ��� ��������, �������� ���
        }
    }

    //����� ���������� ������
    public void Deactivate()
    {
        if (!isActive) return;

        PopulationManager.Instance.ReleasePeasants(requiredPeople);
        isActive = false;
        Debug.Log($"������ {name} ��������������, ��������� �����������");
    }

    //��������������� �����, ������� ��������, ���� �� ������ � ���������� ������
    public bool IsRoadAtCell(Vector3Int cell)
    {
        Tilemap buildTilemap = Builder.Instance.buildTilemap;
        return buildTilemap.HasTile(cell);
    }

    //�������� ���������� �� ������ �� ������ �� ������
    public bool IsStorageNearby(Vector3Int startCell, int maxDistance = 20)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            //���������, ���� �� ����� � ���� ������
            if (IsStorageAtCell(current))
            {
                return true; // ����� ������
            }

            //����������, ���� �������� �����
            if (Vector3Int.Distance(current, startCell) > maxDistance)
                continue;

            //���������� �������� ������
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

        return false; //����� �� ������ � ��������
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
