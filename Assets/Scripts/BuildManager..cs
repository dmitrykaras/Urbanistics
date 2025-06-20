using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    public GameObject housePrefab;          // ��� ������ (���)
    public GridLayout gridLayout;           // ����� (������ "Grid")

    private GameObject currentPrefab;       // ������ ��������� ������ ��� �������������

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SelectHouse()
    {
        currentPrefab = housePrefab;
    }

    public void PlaceAt(Vector3Int cellPosition)
    {
        if (currentPrefab != null && gridLayout != null)
        {
            // ��������� ���������� ������ � ������� � ����
            Vector3 worldPos = gridLayout.CellToWorld(cellPosition);
            Instantiate(currentPrefab, worldPos, Quaternion.identity);
        }
    }
}
