using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject dropdownPanel; //����������� ������

    private static bool isPointerOverMenu = false; //��������� ��������� �� ���� = false (����������)

    //������������ ������
    private void Start()
    {
        DropdownManager.Instance.RegisterPanel(dropdownPanel);
    }


    public void OnPointerEnter(PointerEventData eventData) //��������� �� ������
    {
        isPointerOverMenu = true;
        DropdownManager.Instance.OpenExclusive(dropdownPanel);
    }

    public void OnPointerExit(PointerEventData eventData) //�������� ������� ������� ������
    {
        isPointerOverMenu = false;
        Invoke(nameof(CheckMouseExit), 0.15f); //��������� ��������
    }

    private void CheckMouseExit() //�������� ������ ����� �� �������
    {
        if (!isPointerOverMenu) 
        {
            dropdownPanel.SetActive(false); //�� ����������
        }
    }
}
