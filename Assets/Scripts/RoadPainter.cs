using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPainter : MonoBehaviour
{
    [Header("�������� ���������")]
    public Tilemap roadTilemap; //Tilemap, �� ������� ������ ������
    public TileBase roadTile; //��� ���� ������ (����� RuleTile ��� ����-��������)
    public Transform ghost; //������-������� (������) � optional
    public Camera mainCamera;

    [Header("���������")]
    public KeyCode toggleKey = KeyCode.R;
    public int brushSize = 1; //1 = ���� ������, 2 = 3x3 � �.�. (��������� ����������)
    public float placeInterval = 0.02f; //����������� �������� ����� ������������ ��� drag
    public bool requireHoldMouse = true; //true � ������ ������ ��� ������� ���

    //������� �� ����
    public ResourceCost[] costPerTile; // ���� � ���� ���� ResourceCost struct

    private bool isPainting = false; //��������� � ���������� ������ ������������� �����
    private float lastPlaceTime = 0f;
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, 0);

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (ghost != null) ghost.gameObject.SetActive(false);
    }

    private void Update()
    {
        //toggle �����
        if (Input.GetKeyDown(toggleKey))
        {
            isPainting = !isPainting;
            if (!isPainting)
            {
                if (ghost != null) ghost.gameObject.SetActive(false);
            }
        }

        if (!isPainting) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = Builder.Instance.buildTilemap.WorldToCell(mouseWorld);
        Vector3 cellCenterWorld = Builder.Instance.buildTilemap.GetCellCenterWorld(cell);

        //���������� ghost ��� �����
        if (ghost != null)
        {
            ghost.position = cellCenterWorld;
            ghost.gameObject.SetActive(true);
        }

        bool shouldPaint = !requireHoldMouse || Input.GetMouseButton(0);

        if (shouldPaint)
        {
            //�� ������ ����, ��� placeInterval
            if (Time.time - lastPlaceTime < placeInterval) return;

            //���� brushSize > 1, ������ ���� �� �������
            List<Vector3Int> cellsToPlace = GetBrushCells(cell, brushSize);

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
                //����� ��������� ����/�������� UI "��� ��������"
            }
        }

        //����� ������ ������� ����
        if (Input.GetMouseButtonDown(1))
        {
            isPainting = false;
            if (ghost != null) ghost.gameObject.SetActive(false);
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
        roadTilemap.SetTile(cell, roadTile);

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
}
