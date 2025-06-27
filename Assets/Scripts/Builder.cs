using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class Builder : MonoBehaviour
{
    public Camera mainCamera;
    public Tilemap buildTilemap;

    public GameObject[] buildingPrefabs;     // ������ �������� ������
    public GameObject ghostBuildingPrefab;   // ��� �������� ������ (�������)

    public Button bulldozerButton;

    public AudioClip buildSound;
    public AudioClip destroySound;

    private AudioSource audioSource;
    private Image bulldozerButtonImage;

    private GameObject ghostInstance;
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    public bool bulldozerMode = false;

    private Color normalColor = Color.white;
    private Color activeColor = new Color(1f, 0.4f, 0.4f, 1f);

    private int currentBuildingIndex = 0;   // ��������� ������

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();

        UpdateBulldozerButtonColor();

        SpawnGhost(buildingPrefabs[currentBuildingIndex]);
    }

    void Update()
    {
            if (EventSystem.current.IsPointerOverGameObject())
        return;
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
                    Instantiate(buildingPrefabs[currentBuildingIndex], placePosition, Quaternion.identity);
                    occupiedCells.Add(cellPosition);
                    PlaySound(buildSound);
                }
                else
                {
                    Debug.Log("��� ������ ��� ������!");
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
                    Debug.Log("������ �������");
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
        if (bulldozerButtonImage == null) return;

        bulldozerButtonImage.color = bulldozerMode ? activeColor : normalColor;
    }

    public void SelectBuilding(int index)
    {
        if (index < 0 || index >= buildingPrefabs.Length)
        {
            Debug.LogWarning("�������� ������ ������");
            return;
        }

        currentBuildingIndex = index;

        // ������� ������ ������� � ������ ����� ��� ��������� ������
        if (ghostInstance != null)
            Destroy(ghostInstance);

        SpawnGhost(buildingPrefabs[currentBuildingIndex]);

        Debug.Log("������� ������: " + buildingPrefabs[currentBuildingIndex].name);
    }

    private void SpawnGhost(GameObject buildingPrefab)
    {
        // ������������, ��� � ������� ������ ���� ������-������� � ��������� "Ghost"
        string ghostName = buildingPrefab.name + "Ghost";
        GameObject ghostPrefab = Resources.Load<GameObject>(ghostName);

        if (ghostPrefab == null)
        {
            Debug.LogWarning("�� ������ ������� ��� " + buildingPrefab.name + ". ���������� ������� ������ � �������������.");

            // ������ ����� � ������ � ��������������
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

        // ��������� ���������� � ��������
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
}
