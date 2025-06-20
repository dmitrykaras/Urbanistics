using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    public GameObject housePrefab;          // Что строим (дом)
    public GridLayout gridLayout;           // Сетка (объект "Grid")

    private GameObject currentPrefab;       // Теклый выбранный объект для строительства

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
            // Переводим координаты клетки в позицию в мире
            Vector3 worldPos = gridLayout.CellToWorld(cellPosition);
            Instantiate(currentPrefab, worldPos, Quaternion.identity);
        }
    }
}
