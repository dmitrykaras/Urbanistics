using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    public GridLayout gridLayout;

    [Header("������� ������")]
    public GameObject housePrefab;
    public GameObject roadPrefab;

    private GameObject currentPrefab;

    private bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }


    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = gridLayout.WorldToCell(mouseWorldPos);
            PlaceAt(cellPos);
        }
    }


    /// <summary>
    /// ����� �������� ������� ��� �������������
    /// </summary>
    public void SelectPrefab(GameObject prefab)
    {
        currentPrefab = prefab;
    }

    /// <summary>
    /// ���������� ������ �� ��������� ������
    /// </summary>
    public void PlaceAt(Vector3Int cellPosition)
    {
        if (currentPrefab != null && gridLayout != null)
        {
            // ����� ������ (� �� � ����)
            Vector3 cellWorldPos = gridLayout.CellToWorld(cellPosition);
            Vector3 offset = gridLayout.cellSize / 2f;

            Instantiate(currentPrefab, cellWorldPos + offset, Quaternion.identity);
        }
    }

    // ����� �������� ������� ������ ��� UI-������
    public void SelectHouse() => SelectPrefab(housePrefab);
    public void SelectRoad() => SelectPrefab(roadPrefab);


}
