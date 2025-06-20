using UnityEngine;

public class BuildInput : MonoBehaviour
{
    public Grid grid; // ������ �� ������ � Tilemap/������

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ��������� ������� ���� � ������� ����������
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            // ���������� ������ �� �����
            Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);

            // �������� ���������
            BuildManager.Instance.PlaceAt(cellPos);
        }
    }
}
