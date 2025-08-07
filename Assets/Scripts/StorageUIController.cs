using UnityEngine;
using TMPro;

public class StorageUIController : MonoBehaviour
{
    public static StorageUIController Instance;

    [Header("�������� ���������")]
    public GameObject uiPanel;
    public TMP_Text woodText, stoneText, woolText, ironText;

    private void Awake()
    {
        Instance = this;
        uiPanel.SetActive(false);
    }

    private void Update()
    {
        //��������� ������ ���� ����� ���� ����� ������� � �������� �������
        if (uiPanel.activeSelf)
        {
            UpdateUI();
        }
    }


    //��� ������� ��������� ��������� � ���������� ������
    public void OpenUI()
    {
        UpdateUI();
        uiPanel.SetActive(true);
    }

    //��� ������� �� ���������� ������
    public void CloseUI()
    {
        uiPanel.SetActive(false);
    }

    //���������� ����������
    public void UpdateUI()
    {
        woodText.text = $"���������: {ResourceStorage.Instance.GetAmount(ResourceType.Wood)}";
        stoneText.text = $"������: {ResourceStorage.Instance.GetAmount(ResourceType.Stone)}";
        woolText.text = $"������: {ResourceStorage.Instance.GetAmount(ResourceType.Wool)}";
        ironText.text = $"������: {ResourceStorage.Instance.GetAmount(ResourceType.Iron)}";
    }

}
