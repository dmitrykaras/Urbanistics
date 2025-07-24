using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private bool isUpgradeMode = false;

    private void Awake()
    {
        Instance = this;
    }

    public void EnableUpgradeMode()
    {
        isUpgradeMode = true;
    }

    private void Update()
    {
        if (!isUpgradeMode) return;

        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);
            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null)
            {
                House house = hit.GetComponent<House>();
                if (house != null)
                {
                    house.TryUpgrade();
                }
            }

            isUpgradeMode = false;
        }
    }
}
