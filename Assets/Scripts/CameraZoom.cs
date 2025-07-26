using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraZoom : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap tilemap;

    [Header("Настройки зума")]
    public float zoomSpeed = 5f; //насколько быстро увеличивается/уменьшается масштаб
    public float minZoom = 3f; //минимальное значение масштаба
    public float maxZoom = 13.5f; //максимальное значение масштаба

    private Camera cam;

    private Bounds tilemapBounds;

    void Start()
    {
        cam = Camera.main; //получаем ссылку на основную камеру
        tilemap.CompressBounds(); //сжимаем пустые ячейки
        tilemapBounds = tilemap.localBounds;
    }

    void Update()
    {
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
