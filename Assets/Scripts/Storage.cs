using UnityEngine;

public class Storage : MonoBehaviour
{
    //при нажатии на объект вызывает OpenUI и включает интерфейс
    private void OnMouseDown()
    {
        if (Builder.Instance.bulldozerMode)
            return;
        StorageUIController.Instance.OpenUI();
    }
}
