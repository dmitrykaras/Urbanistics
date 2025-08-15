using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public static BuildingInfo Instance { get; private set; }

    [Header("UI элементы")]
    public GameObject panel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI workersText;
    public TextMeshProUGUI statusText;
    public Button upgradeButton;
    public Button destroyButton;

    public GameObject currentBuilding;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(Upgrade);

        if (destroyButton != null)
            destroyButton.onClick.AddListener(DestroyBuilding);
    }

    private void Update()
    {
        if (!CursorMode.Instance.CursorModeRun)
        {
            HideUIBuilding();
        }
    }

    public void ShowUIBuilding(GameObject building)
    {
        if (building == null || !building.activeInHierarchy)
            return;

        Renderer rend = building.GetComponent<Renderer>();
        if (rend != null && !rend.enabled)
            return;

        if (building.layer == LayerMask.NameToLayer("RoadLayer"))
            return;

        currentBuilding = building;

        ResourceProducer rp = building.GetComponent<ResourceProducer>();
        if (rp != null )
        {
            nameText.text = rp.buildingName;
            workersText.text = "More workers are needed: " + (rp.requiredPeople - PopulationManager.Instance.freePeasants);
            statusText.text = rp.isActive ? "Job status: works!!!" : "Job status: not works";
        }

        House house = building.GetComponent<House>();
        if ( house != null )
        {
            nameText.text = house.buildingName;
            workersText.text = "Lives here: " + house.currentCitizens;
            statusText.text = "They live, they get high";
        }
        
        panel.SetActive(true);
    }

    public void HideUIBuilding()
    {
        panel.SetActive(false);
    }

    public void Upgrade()
    {
        if (currentBuilding == null) return;

        House house = currentBuilding.GetComponent<House>();
        if (house != null)
        {
            house.TryUpgrade();
            panel.SetActive(false);
        }
    }

    public void DestroyBuilding()
    {
        if (currentBuilding == null) return;

        Builder.Instance.DestroySpecificBuilding(currentBuilding);
        currentBuilding = null;
        HideUIBuilding();
    }
}
