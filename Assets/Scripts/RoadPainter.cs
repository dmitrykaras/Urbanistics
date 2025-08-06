using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPainter : MonoBehaviour
{
    public static RoadPainter Instance { get; private set; }

    [Header("�������� ���������")]
    public Tilemap roadTilemap; //Tilemap, �� ������� ������ ������
    public TileBase RoadPrefab; //��� ���� ������ (����� RuleTile ��� ����-��������)
    public Transform RoadGhost; //������-������� (������) � optional
    public Camera mainCamera;

    [Header("���������")]
    public KeyCode toggleKey = KeyCode.R;
    public int brushSize = 1; //1 = ���� ������, 2 = 3x3 � �.�. (��������� ����������)
    public float placeInterval = 0.02f; //����������� �������� ����� ������������ ��� drag
    public bool requireHoldMouse = true; //true � ������ ������ ��� ������� ���

    //������� �� ����
    public ResourceCost[] costPerTile; // ���� � ���� ���� ResourceCost struct

    public bool isPainting = false; //��������� � ���������� ������ ������������� �����
    private float lastPlaceTime = 0f;
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    public bool runningRoadMode = false;

    public GameObject RoadGhostInstance;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        GameObject ghost = Resources.Load<GameObject>("RoadGhost");
        if (ghost != null)
            RoadGhostInstance = Instantiate(ghost);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        RoadGhostRunning(); //��������� ������ ���� ������� RoadGhost

        //toggle �����
        if (Input.GetKeyDown(toggleKey))
        {
            isPainting = !isPainting;
            if (!isPainting)
            {
                if (RoadGhost != null) RoadGhost.gameObject.SetActive(false);
            }

            if (isPainting)
            {
                runningRoadMode = true;
                Debug.Log("RoadBuildingMode running");
            }
            else
            {
                runningRoadMode = false;
                Debug.Log("RoadBuildingMode NOT running");
            }
        }

        if (!isPainting) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);
        Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

        //���������� RoadGhost ��� �����
        if (RoadGhost != null)
        {
            RoadGhost.position = placePosition;
            RoadGhost.gameObject.SetActive(true);
        }

        bool shouldPaint = !requireHoldMouse || Input.GetMouseButton(0);

        if (shouldPaint)
        {
            //�� ������ ����, ��� placeInterval
            if (Time.time - lastPlaceTime < placeInterval) return;

            //���� brushSize > 1, ������ ���� �� �������
            List<Vector3Int> cellsToPlace = GetBrushCells(cellPosition, brushSize);

            //���� ����� � ��������� �������� �������� ����� ���, ��� ��������� ��� ������
            if (CanPlaceBatch(cellsToPlace))
            {
                foreach (var c in cellsToPlace)
                {
                    //�������� ��������� ���������� �� ��� �� cell ������ ��� �����������
                    if (c == lastPlacedCell && brushSize == 1)
                        continue;

                    PlaceRoadAt(c);
                    lastPlacedCell = c;
                    lastPlaceTime = Time.time;
                }
            }
            else
            {
                Debug.Log("�� ������� �������� �� �������� ������");
            }
        }
        //����������� ����� ������� ����
        int wheel = (int)Input.mouseScrollDelta.y;
        if (wheel != 0)
        {
            brushSize = Mathf.Clamp(brushSize + wheel, 1, 9); //�������� �� 1 �� 9
        }
    }

    private List<Vector3Int> GetBrushCells(Vector3Int center, int brush)
    {
        var list = new List<Vector3Int>();
        int radius = brush / 2; // brush 1 => radius 0; brush 3 => radius 1
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                list.Add(new Vector3Int(center.x + x, center.y + y, center.z));
        return list;
    }

    private bool CanPlaceBatch(List<Vector3Int> cells)
    {
        //���������, ��� ���� �� ���� ������ �� ������
        int toPlaceCount = 0;
        foreach (var c in cells)
            if (!RoadManager.Instance.IsRoadAt(c))
                toPlaceCount++;

        if (toPlaceCount == 0) return false;

        // �������� �������� (���� ����)
        if (costPerTile != null && costPerTile.Length > 0)
        {
            // ������ ������ ��������� �������� = costPerTile * toPlaceCount
            var multiplied = new List<ResourceCost>();
            foreach (var rc in costPerTile)
                multiplied.Add(new ResourceCost { resourceType = rc.resourceType, amount = rc.amount * toPlaceCount });

            if (!ResourceStorage.Instance.CanAfford(multiplied.ToArray()))
                return false;
        }
        return true;
    }

    private void PlaceRoadAt(Vector3Int cell)
    {
        if (RoadManager.Instance.IsRoadAt(cell)) return; //��� ������

        //��������� ������� �� ���� ������ (��� ������ ���� ����)
        if (costPerTile != null && costPerTile.Length > 0)
        {
            if (!ResourceStorage.Instance.CanAfford(costPerTile))
            {
                return;
            }
            ResourceStorage.Instance.DeductResources(costPerTile);
        }

        //������ ����
        roadTilemap.SetTile(cell, RoadPrefab);

        //��������� � ���������� ���������
        RoadManager.Instance.AddRoad(cell);

        //���� ����������� ����������� (RuleTile) � ������ �������� �����:
        UpdateNeighbors(cell);
    }

    private void UpdateNeighbors(Vector3Int cell)
    {
        Vector3Int[] dirs = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) };
        foreach (var d in dirs)
            roadTilemap.RefreshTile(cell + d);
        roadTilemap.RefreshTile(cell);
    }

    //��������� ������� RoadGhost 
    private void RoadGhostRunning()
    {
        if (isPainting)
        {
            Builder.Instance.DestroyGhost();

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);
            Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

            if (RoadGhostInstance != null)
            {
                RoadGhostInstance.transform.position = placePosition;
                RoadGhostInstance.SetActive(true);
            }
        }
        else
        {
            if (RoadGhostInstance != null)
            {
                RoadGhostInstance.SetActive(false);
            }
        }
    }
}
