using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject dropdownPanel; //всплывающая панель

    private static bool isPointerOverMenu = false; //указатель находится на меню = false (переменная)

    //регистрируем панель
    private void Start()
    {
        DropdownManager.Instance.RegisterPanel(dropdownPanel);
    }


    public void OnPointerEnter(PointerEventData eventData) //указатель на кнопке
    {
        isPointerOverMenu = true;
        DropdownManager.Instance.OpenExclusive(dropdownPanel);
    }

    public void OnPointerExit(PointerEventData eventData) //укзатель покинул облость кнопки
    {
        isPointerOverMenu = false;
        Invoke(nameof(CheckMouseExit), 0.15f); //небольшая задержка
    }

    private void CheckMouseExit() //проверка выхода мышки за облость
    {
        if (!isPointerOverMenu) 
        {
            dropdownPanel.SetActive(false); //не показывать
        }
    }
}
