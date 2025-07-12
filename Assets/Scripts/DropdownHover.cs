using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject dropdownPanel; //����������� ������
    private static bool isPointerOverMenu = false; //��������� ��������� �� ���� = false (����������)

    public void OnPointerEnter(PointerEventData eventData) //��������� �� ������
    {
        isPointerOverMenu = true; 
        dropdownPanel.SetActive(true); //��������
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
