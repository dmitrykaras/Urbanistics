using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap tilemap;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 13.5f;

    [Header("Pan (перетаскивание)")]
    public float panSpeed = 1f;

    private Camera cam;
    private Vector3 dragStartPos;
    private bool isDragging = false;
    private Bounds tilemapBounds;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (tilemap != null)
            tilemapBounds = tilemap.localBounds;
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();
        ClampCamera();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragStartPos = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButton(2) && isDragging)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offset = dragStartPos - currentPos;
            transform.position += offset;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }
    }

    void ClampCamera()
    {
        if (tilemap == null) return;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        float minX = tilemapBounds.min.x + horzExtent;
        float maxX = tilemapBounds.max.x - horzExtent;
        float minY = tilemapBounds.min.y + vertExtent;
        float maxY = tilemapBounds.max.y - vertExtent;

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);
        clampedPos.z = transform.position.z;

        transform.position = clampedPos;
    }
}
