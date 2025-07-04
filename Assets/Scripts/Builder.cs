using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [Header("Основные ссылки")]
    public Camera mainCamera; // Камера
    public Tilemap buildTilemap; // Сетка

    [Header("Данные зданий")]
    public BuildingData[] buildingDatas; // Все здания с префабами и стоимостью

    [Header("Настройки строительства")]
    public GameObject ghostBuildingPrefab; // Призрак здания
    public AudioClip buildSound;
    public AudioClip destroySound;

    private AudioSource audioSource;
    private GameObject ghostInstance;
    private int currentBuildingIndex = 0;
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    // Ресурсы игрока
    private Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("Данные Bulldozer Mode")]
    public Button bulldozerButton;
    private Image bulldozerButtonImage;
    public bool bulldozerMode = false;

    private Color normalColor = Color.white;
    private Color activeColor = new Color(1f, 0.4f, 0.4f, 1f);

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();
        UpdateBulldozerButtonColor();

        // Инициализируем ресурсы
        playerResources[ResourceType.Wood] = 100;
        playerResources[ResourceType.Stone] = 50;

        SpawnGhost(buildingDatas[currentBuildingIndex].prefab);
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = buildTilemap.WorldToCell(mouseWorldPos);
        Vector3 placePosition = buildTilemap.GetCellCenterWorld(cellPosition);

        if (!bulldozerMode)
        {
            if (ghostInstance != null)
                ghostInstance.transform.position = placePosition;

            if (Input.GetMouseButtonDown(0))
            {
                if (!occupiedCells.Contains(cellPosition))
                {
                    BuildingData data = buildingDatas[currentBuildingIndex];

                    if (CanAfford(data.cost))
                    {
                        Instantiate(data.prefab, placePosition, Quaternion.identity);
                        DeductResources(data.cost);
                        occupiedCells.Add(cellPosition);
                        PlaySound(buildSound);
                        Debug.Log("Построено: " + data.prefab.name);
                    }
                    else
                    {
                        Debug.Log("Недостаточно ресурсов для постройки!");
                    }
                }
                else
                {
                    Debug.Log("Эта клетка уже занята!");
                }
            }

            if (ghostInstance != null)
                ghostInstance.SetActive(true);
        }
        else
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);

            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hitCollider = Physics2D.OverlapPoint(placePosition);
                if (hitCollider != null && hitCollider.gameObject.CompareTag("Building"))
                {
                    Destroy(hitCollider.gameObject);
                    occupiedCells.Remove(cellPosition);
                    PlaySound(destroySound);
                    Debug.Log("Здание удалено");
                }
            }
        }
    }

    public void ToggleBulldozerMode()
    {
        bulldozerMode = !bulldozerMode;
        Debug.Log("Bulldozer mode: " + (bulldozerMode ? "ON" : "OFF"));
        UpdateBulldozerButtonColor();
    }

    private void UpdateBulldozerButtonColor()
    {
        if (bulldozerButtonImage == null)
            bulldozerButtonImage = bulldozerButton.GetComponent<Image>();

        bulldozerButtonImage.color = bulldozerMode ? activeColor : normalColor;
    }

    public void SelectBuilding(int index)
    {
        if (index < 0 || index >= buildingDatas.Length)
        {
            Debug.LogWarning("Неверный индекс здания");
            return;
        }

        currentBuildingIndex = index;

        if (ghostInstance != null)
            Destroy(ghostInstance);

        SpawnGhost(buildingDatas[currentBuildingIndex].prefab);

        Debug.Log("Выбрано здание: " + buildingDatas[currentBuildingIndex].prefab.name);
    }

    private void SpawnGhost(GameObject buildingPrefab)
    {
        string ghostName = buildingPrefab.name + "Ghost";
        GameObject ghostPrefab = Resources.Load<GameObject>(ghostName);

        if (ghostPrefab == null)
        {
            Debug.LogWarning("Не найден призрак для " + buildingPrefab.name + ". Используем обычный префаб с прозрачностью.");
            ghostInstance = Instantiate(buildingPrefab);
            SetGhostTransparency(ghostInstance);
        }
        else
        {
            ghostInstance = Instantiate(ghostPrefab);
        }
    }

    private void SetGhostTransparency(GameObject obj)
    {
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var rend in renderers)
        {
            Color c = rend.color;
            c.a = 0.4f;
            rend.color = c;
        }

        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    private bool CanAfford(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            if (!playerResources.ContainsKey(item.resourceType) || playerResources[item.resourceType] < item.amount)
                return false;
        }
        return true;
    }

    private void DeductResources(ResourceCost[] cost)
    {
        foreach (var item in cost)
        {
            playerResources[item.resourceType] -= item.amount;
        }
    }
}