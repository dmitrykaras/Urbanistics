using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Настройки зума")]
    public float zoomSpeed = 5f; //насколько быстро увеличивается/уменьшается масштаб
    public float minZoom = 3f; //минимальное значение масштаба
    public float maxZoom = 13.5f; //максимальное значение масштаба

    private Camera cam;

    void Start()
    {
        cam = Camera.main; //получаем ссылку на основную камеру
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); //получаем ввод от колёсика мыши
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed; //изменяем размер ортографической камеры
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom); //ограничиваем значения
        }
    }
}
