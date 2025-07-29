using UnityEngine;

public class BoostingButton : MonoBehaviour
{
    private House selectedHouse;

    public void OnBoostingButtonClick()
    {
        BoostingManager.Instance.ToggleBoostingMode();
    }

    public void SetTarget(House house)
    {
        selectedHouse = house;
        Debug.Log("Цель установлена: " + house.name);
    }
}
