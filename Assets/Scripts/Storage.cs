using UnityEngine;

public class Storage : MonoBehaviour
{
    //��� ������� �� ������ �������� OpenUI � �������� ���������
    private void OnMouseDown()
    {
        StorageUIController.Instance.OpenUI();
    }
}
