using UnityEngine;
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

    private bool isBoostingMode = false; //���� ��� ���/���� ������ ���������

    //������ � �����
    private Image BoostingButtonImage;
    public Button BoostingButton;
    public GameObject BoostingGhostInstance;

    private void Start()
    {
        GameObject ghost = Resources.Load<GameObject>("BoostingGhost");
        if (ghost != null)
            BoostingGhostInstance = Instantiate(ghost);
    }

    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode; //���/����
        Debug.Log("Boosting mode: " + isBoostingMode);
        UpdateBoostingButtonColor();
    }

    private void UpdateBoostingButtonColor()
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

            //    if (BoostingManager.Instance.BoostingGhostInstance != null)
            //{
            //    BoostingManager.Instance.BoostingGhostInstance.transform.position = placePosition;
            //    BoostingManager.Instance.BoostingGhostInstance.SetActive(true);
            //}
}
