using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class RoadPainter : MonoBehaviour
{
    public static RoadPainter Instance { get; private set; }

    [Header("�������� ���������")]
    public Tilemap buildTilemap; //Tilemap, �� ������� ������ ������
    public TileBase RoadTile; //��� ���� ������ (����� RuleTile ��� ����-��������)
    public Transform RoadGhost; //������-������� (������) � optional
    public Camera mainCamera;

    [Header("���������")]
    public KeyCode toggleKey = KeyCode.R;
    public int brushSize = 1; //1 = ���� ������, 2 = 3x3 � �.�. (��������� ����������)
    public float placeInterval = 0.02f; //����������� �������� ����� ������������ ��� drag
    public bool requireHoldMouse = true; //true � ������ ������ ��� ������� ���

    [Header("���������")]
    //������� �� ����
    public ResourceCost[] costPerTile; // ���� � ���� ���� ResourceCost struct

    [Header("��������� ���������")]
    public bool isPainting = false; //��������� � ���������� ������ ������������� �����
    private float lastPlaceTime = 0f;

    public GameObject RoadGhostInstance;

    [Header("�����")]
    public AudioClip sand;

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

    void Update()
    {
        //���� ����� �������� �� UI, �� ������������ ����
        if (EventSystem.current.IsPointerOverGameObject()) return;

        RoadGhostRunning(); //��������� ������ ���� ������� RoadGhost

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
            if (Time.time - lastPlaceTime < placeInterval) return;

            if (!RoadManager.Instance.IsRoadAt(cellPosition))
            {
                if (ResourceStorage.Instance.CanAfford(costPerTile))
                {
                    ResourceStorage.Instance.DeductResources(costPerTile);
                    PlaceRoadAt(cellPosition);
                    Builder.Instance.PlaySound(sand);
                    lastPlaceTime = Time.time;
                }
                else
                {
                    Debug.Log("�� ������� �������� �� ��������� ������");
                }
            }
        }

    }

    public void ToggleRoadPainter()
    {
        isPainting = !isPainting;
        Debug.Log("RoadBuilderMode: " + (isPainting ? "ON" : "OFF"));
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
    }

    //������ ������
    private void PlaceRoadAt(Vector3Int cell)
    {
        if (RoadManager.Instance.IsRoadAt(cell)) return; //��� ������

        //��������� ������� �� ���� ������
        if (costPerTile != null && costPerTile.Length > 0)
        {
            if (!ResourceStorage.Instance.CanAfford(costPerTile))
            {
                return;
            }
            ResourceStorage.Instance.DeductResources(costPerTile);
        }

        //������ ����
        buildTilemap.SetTile(cell, RoadTile);

        //��������� � ���������� ���������
        RoadManager.Instance.AddRoad(cell);

        Builder.Instance.PlaySound(sand);
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

    //���������� ������ ������������� �����
    public void DisableRoadMode()
    {
        if (isPainting)
        {
            isPainting = false;
            Debug.Log("RoadBuilderMode: " + (isPainting ? "ON" : "OFF"));
            RoadGhostInstance.SetActive(false);
        }
    }
}
