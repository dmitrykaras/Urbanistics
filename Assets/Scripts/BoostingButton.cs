using UnityEngine;

//������ ��������� ������
public class BoostingButton : MonoBehaviour
{
    private House selectedHouse;

    public void TryBoostSelected()
    {
        //���� �������� ������ ����������, �� ������� ��� ������
        if (selectedHouse != null) 
        {
            selectedHouse.TryUpgrade();
        }
    }

    public void SetTarget(House house)
    {
        selectedHouse = house;
    }
}
