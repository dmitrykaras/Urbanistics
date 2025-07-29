using UnityEngine;

public class BoostingManager : MonoBehaviour
{
    public static BoostingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private bool isBoostingMode = false; //флаг для вкл/выкл режима аппгрейда

    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode; //вкл/выкл
        Debug.Log("Boosting mode: " + isBoostingMode);
    }

    //
    public bool IsBoostingMode()
    {
        return isBoostingMode;
    }
}
