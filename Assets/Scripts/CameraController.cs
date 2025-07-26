using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Настройки перетаскивания")]
    public float dragSpeed = 1f; //скорость перемещения

    private Vector3 dragOrigin; //исходная точка нажатия
    private bool isDragging = false;

    //переменные для границ перемещения
    public Vector2 minCameraPos;
    public Vector2 maxCameraPos;

    private Camera cam; //ссылка на камеру

    public SpriteRenderer mapImage;

    [Header("Настройки зума")]
    public float zoomSpeed = 5f; //насколько быстро увеличивается/уменьшается масштаб
    public float minZoom = 3f; //минимальное значение масштаба
    public float maxZoom = 13.5f; //максимальное значение масштаба


    void Start()
    {
        if (mapImage != null)
        {
            Bounds bounds = mapImage.bounds;
            minCameraPos = bounds.min;
            maxCameraPos = bounds.max;
        }

        cam = Camera.main; //получаем ссылку на основную камеру

        //гранцы перемещения камеры
        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;
    }

    void Update()
    {
        //ПЕРЕТАКСИВАНИЕ
        //нажатие средней кнопки мыши
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPosition = transform.position + difference;

            ApplyCameraBounds(ref newPosition); //применяем границы
            transform.position = newPosition;
        }

        //ЗУМ
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

            //после зума ограничить позицию камеры
            Vector3 clampedPosition = transform.position;
            ApplyCameraBounds(ref clampedPosition);
            transform.position = clampedPosition;
        }
    }

    //границы
    void ApplyCameraBounds(ref Vector3 position)
    {
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        position.x = Mathf.Clamp(position.x, minCameraPos.x + horzExtent, maxCameraPos.x - horzExtent);
        position.y = Mathf.Clamp(position.y, minCameraPos.y + vertExtent, maxCameraPos.y - vertExtent);
    }

}
