using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingPlacer : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject buildingPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
            Vector3 placePos = tilemap.GetCellCenterWorld(cellPos);

            Collider2D hit = Physics2D.OverlapPoint(placePos);
            if (hit == null)
            {
                Instantiate(buildingPrefab, placePos, Quaternion.identity);
            }
        }
    }
}
