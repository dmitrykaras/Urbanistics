using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
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

    [Header("Настройки зума")]
    public float zoomSpeed = 5f; //насколько быстро увеличивается/уменьшается масштаб
    public float minZoom = 3f; //минимальное значение масштаба
    public float maxZoom = 13.5f; //максимальное значение масштаба

    private Bounds tilemapBounds;


    void Start()
    {
        cam = Camera.main; //получаем ссылку на основную камеру


        tilemap.CompressBounds(); //сжимаем пустые ячейки

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
            Vector3 difference = Camera.main.ScreenToWorldPoint(dragOrigin) - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            difference.z = 0;

            transform.position += difference * dragSpeed;

            dragOrigin = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float targetSize = cam.orthographicSize - scroll * zoomSpeed;

            //вычисляем максимально возможный зум, чтобы вся tilemap помещалась в экран
            float cameraAspect = cam.aspect;

            float tilemapWidth = tilemapBounds.size.x;
            float tilemapHeight = tilemapBounds.size.y;

            float maxSizeX = tilemapWidth / (2f * cameraAspect);
            float maxSizeY = tilemapHeight / 2f;

            float maxZoomBasedOnBounds = Mathf.Min(maxSizeX, maxSizeY);

            //предохранитель: если расчет даёт 0 или меньше
            if (maxZoomBasedOnBounds <= 0f)
            {
                maxZoomBasedOnBounds = maxZoom;
            }

            float finalMaxZoom = Mathf.Min(maxZoom, maxZoomBasedOnBounds);

            //ограничиваем масштаб
            cam.orthographicSize = Mathf.Clamp(targetSize, minZoom, finalMaxZoom);
        }
    }
}
