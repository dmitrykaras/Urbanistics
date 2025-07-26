using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraDrag : MonoBehaviour
{
    [Header("Настройки перетаскивания")]
    public float dragSpeed = 1f; //скорость перемещения

    [Header("Границы карты")]
    public Tilemap tilemap; //ссылка на tilemap

    private Vector3 dragOrigin; //исходная точка нажатия
    private bool isDragging = false;

    //переменные для границ перемещения
    private Vector2 minCameraPos;
    private Vector2 maxCameraPos;

    private Camera cam; //ссылка на камеру

    void Start()
    {
        cam = Camera.main;

        //получаем границы tilemap
        Bounds bounds = tilemap.localBounds;

        //гранцы перемещения камеры
        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;

        minCameraPos = new Vector2(bounds.min.x + horizExtent, bounds.min.y + vertExtent);
        maxCameraPos = new Vector2(bounds.min.x - horizExtent, bounds.min.y - vertExtent);
    }

    void Update()
    {
        //нажатие средней кнопки мыши
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        //отпустили кнопку
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        //перетаскивание
        if (isDragging)
        {
            Vector3 difference = cam.ScreenToWorldPoint(dragOrigin) - cam.ScreenToWorldPoint(Input.mousePosition);
            difference.z = 0; //игнорируем ось Z

            Vector3 newPosition = transform.position + difference * dragSpeed;

            //ограничиваем перемещение камеры
            newPosition.x = Mathf.Clamp(newPosition.x, minCameraPos.x, maxCameraPos.x);
            newPosition.y = Mathf.Clamp(newPosition.y, minCameraPos.y, maxCameraPos.y);

            transform.position = newPosition;

            dragOrigin = Input.mousePosition; //обновляем исходную точку
        }
    }
}
