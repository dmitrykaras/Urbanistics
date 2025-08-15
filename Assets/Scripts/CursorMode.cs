using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CursorMode : MonoBehaviour
{
    public static CursorMode Instance { get; private set; }

    public bool CursorModeRun = false;

    public Image cursorButtonImage;

    public Sprite cursorDefaultSprite;
    public Sprite cursorActiveSprite;

    public Camera mainCamera;
    public Tilemap buildTilemap;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorMode();
        }

        //�� �����������, ���� ���� �� UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        TryShowUI();
    }

    public void ToggleCursorMode()
    {
        CursorModeRun = !CursorModeRun;
        Debug.Log("CursorMode: " + (CursorModeRun ? "ON" : "OFF"));

        if (Builder.Instance.ghostInstance != null)
        {
            Builder.Instance.ghostInstance.SetActive(false); ; //������� ���������� ���������� ������, ���� ���
        }
        if (RoadPainter.Instance.RoadGhostInstance != null)
        {
            RoadPainter.Instance.RoadGhostInstance.SetActive(false);
        }
        if (Builder.Instance.bulldozerGhostInstance != null)
        {
            Builder.Instance.bulldozerGhostInstance.SetActive(false);
        }

        if (RoadPainter.Instance.isPainting) RoadPainter.Instance.DisableRoadMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();

        UpdateButtonImage();
    }

    public void TryShowUI()
    {
        //����������� � ���������� ������ ��������
        Vector3Int cellPosition = Builder.Instance.GetMouseCellPosition();

        //�������� ����� ������ � ������� �����������
        Vector3 placePosition = buildTilemap.GetCellCenterWorld(cellPosition);

        if (Input.GetMouseButtonDown(0) && (CursorModeRun))
        {
            //���� ������
            Collider2D hitCollider = Physics2D.OverlapPoint(placePosition);

            if (hitCollider != null)
            {
                GameObject building = hitCollider.gameObject;
                Debug.Log("����� ������: " + building.name);
                BuildingInfo.Instance.ShowUIBuilding(building); 
            }
        }

        //��� � ������� ����
        if (Input.GetMouseButtonDown(1))
            BuildingInfo.Instance.HideUIBuilding();
    }

    private void UpdateButtonImage()
    {
        if (cursorButtonImage != null)
            cursorButtonImage.sprite = CursorModeRun ? cursorActiveSprite : cursorDefaultSprite;
    }

    public void DisableCursorMode()
    {
        CursorModeRun = false;

    }
}
