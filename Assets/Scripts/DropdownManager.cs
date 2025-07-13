using UnityEngine;
using System.Collections.Generic;

public class DropdownManager : MonoBehaviour
{
    public static DropdownManager Instance;

    private List<GameObject> registeredPanels = new List<GameObject>(); //список всех панелей

    //инициализация
    private void Awake()
    {
        //если Instance ещё не назначен — назначаем
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    //регистрируем панель
    public void RegisterPanel(GameObject panel)
    {
        if (!registeredPanels.Contains(panel))
            registeredPanels.Add(panel);
    }

    //закрывает все панели, кроме указанной
    public void OpenExclusive(GameObject panelToOpen)
    {
        foreach (var panel in registeredPanels)
        {
            if (panel != panelToOpen)
                panel.SetActive(false); //закрываем остальные
        }

        panelToOpen.SetActive(true); //оставляем только нужную
    }

    //закрывает все панели
    public void CloseAll()
    {
        foreach (var panel in registeredPanels)
        {
            panel.SetActive(false); //полностью закрывает все панели
        }
    }
}
