using UnityEngine;

public class Storage : MonoBehaviour
{
    //��� ������� �� ������ �������� OpenUI � �������� ���������
    private void OnMouseDown()
    {
        if (Builder.Instance.bulldozerMode)
            return;
        StorageUIController.Instance.OpenUI();
    }
}
