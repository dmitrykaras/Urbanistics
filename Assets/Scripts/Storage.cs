using UnityEngine;

public class Storage : MonoBehaviour
{
    //��� ������� �� ������ �������� OpenUI � �������� ���������
    private void OnMouseDown()
    {
        if (CursorMode.Instance.CursorModeRun)
        {
            StorageUIController.Instance.OpenUI();
        }
        else
        {
            return;
        }
    }
}
