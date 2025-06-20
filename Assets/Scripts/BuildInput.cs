using UnityEngine;

public class BuildInput : MonoBehaviour
{
    public Grid grid; // —сылка на объект с Tilemap/сеткой

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ѕереводим позицию мыши в мировые координаты
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            // ќпредел€ем клетку на сетке
            Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);

            // ѕытаемс€ построить
            BuildManager.Instance.PlaceAt(cellPos);
        }
    }
}
