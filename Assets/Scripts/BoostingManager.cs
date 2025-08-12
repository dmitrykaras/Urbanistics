using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoostingManager : MonoBehaviour
{
    public static BoostingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool isBoostingMode = false; //���� ��� ���/���� ������ ���������
    public bool runningBoostingMode = false;

    //������ � �����
    private Image BoostingButtonImage;
    public Button BoostingButton;
    public GameObject BoostingGhostInstance;

    void Start()
    {
        GameObject ghost = Resources.Load<GameObject>("BoostingGhost");
        if (ghost != null)
            BoostingGhostInstance = Instantiate(ghost);
    }

    void Update()
    {
        //���� ����� �������� �� UI, �� ������������ ����
        if (EventSystem.current.IsPointerOverGameObject()) return;

        BoostingGhost();
    }

    //���-���� Boosting mode
    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode; //���/����
        Debug.Log("Boosting mode: " + (isBoostingMode ? "ON" : "OFF"));
        UpdateBoostingButtonColor();

        if (RoadPainter.Instance.isPainting) RoadPainter.Instance.DisableRoadMode();
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();
    }

    //���������� ����� ������ BoostingButton
    public void UpdateBoostingButtonColor()
    {
        //���� ������ �� ��������� Image ��� �� ����������� � ���� � �� ������
        if (BoostingButtonImage == null)
            BoostingButtonImage = BoostingButton.GetComponent<Image>();
        //����� ���� ������ � ����������� �� ���������
        BoostingButtonImage.color = isBoostingMode ? new Color(0.4f, 0.8f, 0.4f, 1f) : Color.white;
    }

    //������������� ������ � ���� ���������� �� ������ �������
    public bool IsBoostingMode()
    {
        return isBoostingMode;
    }

    //��������� BoostingGhost
    public void BoostingGhost()
    {
        if (isBoostingMode)
        {
            Builder.Instance.DestroyGhost();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //����������� � ���������� ������ ��������
            Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);

            //�������� ����� ������ � ������� �����������
            Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

            if (BoostingGhostInstance != null)
            {
                BoostingGhostInstance.transform.position = placePosition;
                BoostingGhostInstance.SetActive(true);
            }
        }
        else
        {
            if (BoostingGhostInstance != null)
            {
                BoostingGhostInstance.SetActive(false);
            }
        }
    }

    //���������� ������ ��������� ������
    public void DisableBoostingMode()
    {
        if (isBoostingMode)
        {
            isBoostingMode = false;
            Debug.Log("BoostingMode: " + (isBoostingMode ? "ON" : "OFF"));
            UpdateBoostingButtonColor();
            BoostingGhostInstance.SetActive(false);
        }
    }
}
