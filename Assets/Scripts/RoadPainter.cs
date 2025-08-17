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

    [Header("��������� ���������")]
    public bool isPainting = false; //��������� � ���������� ������ ������������� �����

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
        Vector3 mouseWorldPos = Builder.Instance.GetMouseCellPosition();
        Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);
        Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

        //���� ����� �������� �� UI, �� ������������ ����
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRoadPainter();
        }

        RoadGhostRunning(cellPosition, placePosition); //��������� ������ ���� ������� RoadGhost

        if (!isPainting) return;

        //���������� RoadGhost ��� �����
        if (RoadGhost != null)
        {
            RoadGhost.position = placePosition;
            RoadGhost.gameObject.SetActive(true);
        }

        if (Input.GetMouseButtonDown(0) && (!RoadManager.Instance.IsRoadAt(cellPosition)))
        {
            PlaceRoadAt(cellPosition);
        }

    }

    public void ToggleRoadPainter()
    {
        isPainting = !isPainting;
        Debug.Log("RoadBuilderMode: " + (isPainting ? "ON" : "OFF"));
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
        if (CursorMode.Instance.CursorModeRun) CursorMode.Instance.DisableCursorMode();
    }

    //������ ������
    private void PlaceRoadAt(Vector3Int cellPosition)
    {
        if (!Builder.Instance.occupiedCells.Add(cellPosition))
        {
            Debug.Log("���������� ��������� ������: ������ ��� ������!");
            return;
        }

        buildTilemap.SetTile(cellPosition, RoadTile); //������ ����

        RoadManager.Instance.AddRoad(cellPosition); //��������� � ���������� ���������

        Builder.Instance.occupiedCells.Add(cellPosition); //�������� ������ �������

        Builder.Instance.PlaySound(sand); 
    }

    //��������� ������� RoadGhost 
    private void RoadGhostRunning(Vector3Int cellPosition, Vector3 placePosition)
    {
        if (isPainting)
        {
            Builder.Instance.DisablingGhost();

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
