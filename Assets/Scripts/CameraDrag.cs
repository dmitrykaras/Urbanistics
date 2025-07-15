using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    [Header("��������� ��������������")]
    public float dragSpeed = 1f; //�������� �����������

    private Vector3 dragOrigin; //�������� ����� �������
    private bool isDragging = false;

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
            Vector3 difference = Camera.main.ScreenToWorldPoint(dragOrigin) - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            difference.z = 0; //���������� ��� Z

            transform.position += difference * dragSpeed;

            dragOrigin = Input.mousePosition; //��������� �������� �����
        }
    }
}
