using UnityEngine;

public class Storage : MonoBehaviour
{
    //при нажатии на объект вызывает OpenUI и включает интерфейс
    private void OnMouseDown()
    {
        StorageUIController.Instance.OpenUI();
    }
}
