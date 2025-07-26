using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraZoom : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap tilemap;

    [Header("��������� ����")]
    public float zoomSpeed = 5f; //��������� ������ �������������/����������� �������
    public float minZoom = 3f; //����������� �������� ��������
    public float maxZoom = 13.5f; //������������ �������� ��������

    private Camera cam;

    private Bounds tilemapBounds;

    void Start()
    {
        cam = Camera.main; //�������� ������ �� �������� ������
        tilemap.CompressBounds(); //������� ������ ������
        tilemapBounds = tilemap.localBounds;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float targetSize = cam.orthographicSize - scroll * zoomSpeed;

            //��������� ����������� ��������� ���, ����� ��� tilemap ���������� � �����
            float cameraAspect = cam.aspect;

            float tilemapWidth = tilemapBounds.size.x;
            float tilemapHeight = tilemapBounds.size.y;

            float maxSizeX = tilemapWidth / (2f * cameraAspect);
            float maxSizeY = tilemapHeight / 2f;

            float maxZoomBasedOnBounds = Mathf.Min(maxSizeX, maxSizeY);

            //��������������: ���� ������ ��� 0 ��� ������
            if (maxZoomBasedOnBounds <= 0f)
            {
                maxZoomBasedOnBounds = maxZoom;
            }

            float finalMaxZoom = Mathf.Min(maxZoom, maxZoomBasedOnBounds);

            //������������ �������
            cam.orthographicSize = Mathf.Clamp(targetSize, minZoom, finalMaxZoom);
        }


    }
}
