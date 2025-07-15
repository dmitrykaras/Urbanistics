using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("��������� ����")]
    public float zoomSpeed = 5f; //��������� ������ �������������/����������� �������
    public float minZoom = 3f; //����������� �������� ��������
    public float maxZoom = 13.5f; //������������ �������� ��������

    private Camera cam;

    void Start()
    {
        cam = Camera.main; //�������� ������ �� �������� ������
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); //�������� ���� �� ������� ����
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed; //�������� ������ ��������������� ������
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom); //������������ ��������
        }
    }
}
