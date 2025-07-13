using UnityEngine;
using System.Collections.Generic;

public class DropdownManager : MonoBehaviour
{
    public static DropdownManager Instance;

    private List<GameObject> registeredPanels = new List<GameObject>(); //������ ���� �������

    //�������������
    private void Awake()
    {
        //���� Instance ��� �� �������� � ���������
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    //������������ ������
    public void RegisterPanel(GameObject panel)
    {
        if (!registeredPanels.Contains(panel))
            registeredPanels.Add(panel);
    }

    //��������� ��� ������, ����� ���������
    public void OpenExclusive(GameObject panelToOpen)
    {
        foreach (var panel in registeredPanels)
        {
            if (panel != panelToOpen)
                panel.SetActive(false); //��������� ���������
        }

        panelToOpen.SetActive(true); //��������� ������ ������
    }

    //��������� ��� ������
    public void CloseAll()
    {
        foreach (var panel in registeredPanels)
        {
            panel.SetActive(false); //��������� ��������� ��� ������
        }
    }
}
