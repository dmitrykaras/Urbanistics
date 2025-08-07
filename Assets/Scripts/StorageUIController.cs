using UnityEngine;
using TMPro;

public class StorageUIController : MonoBehaviour
{
    public static StorageUIController Instance;

    [Header("Элементы интефейса")]
    public GameObject uiPanel;
    public TMP_Text woodText, stoneText, woolText, ironText;

    private void Awake()
    {
        Instance = this;
        uiPanel.SetActive(false);
    }

    private void Update()
    {
        //обновляем каждый кадр чтобы было видно ресурсы в реальном времени
        if (uiPanel.activeSelf)
        {
            UpdateUI();
        }
    }


    //при вызвове обновлять интерфейс и показывать объект
    public void OpenUI()
    {
        UpdateUI();
        uiPanel.SetActive(true);
    }

    //при вызвове не показывать объект
    public void CloseUI()
    {
        uiPanel.SetActive(false);
    }

    //обновление интерфейса
    public void UpdateUI()
    {
        woodText.text = $"Древесина: {ResourceStorage.Instance.GetAmount(ResourceType.Wood)}";
        stoneText.text = $"Камень: {ResourceStorage.Instance.GetAmount(ResourceType.Stone)}";
        woolText.text = $"Шерсть: {ResourceStorage.Instance.GetAmount(ResourceType.Wool)}";
        ironText.text = $"Железо: {ResourceStorage.Instance.GetAmount(ResourceType.Iron)}";
    }

}
