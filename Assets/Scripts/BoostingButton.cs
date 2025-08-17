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
        if (BoostingManager.Instance.isBoostingMode)
        {
            selectedHouse = house;
            Debug.Log("Цель установлена: " + house.name);
        }
    }
}
