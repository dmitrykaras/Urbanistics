using UnityEngine;

//кнопка улучшения зданий
public class BoostingButton : MonoBehaviour
{
    private House selectedHouse;

    public void TryBoostSelected()
    {
        //если выбраное здание существует, то улучаем его нажаии
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
