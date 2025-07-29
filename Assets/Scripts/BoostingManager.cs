using UnityEngine;

public class BoostingManager : MonoBehaviour
{
    public static BoostingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // или выдавай ошибку
    }

    private bool isBoostingMode = false;

    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode;
        Debug.Log("Boosting mode: " + isBoostingMode);
    }

    public bool IsBoostingMode()
    {
        return isBoostingMode;
    }
}
