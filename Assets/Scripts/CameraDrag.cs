using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraDrag : MonoBehaviour
{
    [Header("��������� ��������������")]
    public float dragSpeed = 1f; //�������� �����������

    [Header("������� �����")]
    public Tilemap tilemap; //������ �� tilemap

    private Vector3 dragOrigin; //�������� ����� �������
    private bool isDragging = false;

    //���������� ��� ������ �����������
    private Vector2 minCameraPos;
    private Vector2 maxCameraPos;

    private Camera cam; //������ �� ������

    void Start()
    {
        cam = Camera.main;

        //�������� ������� tilemap
        Bounds bounds = tilemap.localBounds;

        //������ ����������� ������
        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;

        minCameraPos = new Vector2(bounds.min.x + horizExtent, bounds.min.y + vertExtent);
        maxCameraPos = new Vector2(bounds.min.x - horizExtent, bounds.min.y - vertExtent);
    }

    void Update()
    {
        //������� ������� ������ ����
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        //��������� ������
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        //��������������
        if (isDragging)
        {
            Vector3 difference = cam.ScreenToWorldPoint(dragOrigin) - cam.ScreenToWorldPoint(Input.mousePosition);
            difference.z = 0; //���������� ��� Z

            Vector3 newPosition = transform.position + difference * dragSpeed;

            //������������ ����������� ������
            newPosition.x = Mathf.Clamp(newPosition.x, minCameraPos.x, maxCameraPos.x);
            newPosition.y = Mathf.Clamp(newPosition.y, minCameraPos.y, maxCameraPos.y);

            transform.position = newPosition;

            dragOrigin = Input.mousePosition; //��������� �������� �����
        }
    }
}
