using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GrassSpawner : MonoBehaviour
{
    [Header("�������� ������")]
    public Tilemap tilemap;              // ������ �� Tilemap
    public GameObject grassPrefab;       // ������ �����
    public int width = 20;               // ������ ����� (� �������)
    public int height = 20;              // ������ �����
    public float cellSize = 1f;

    void Start()
    {
        SpawnGrass();
    }

    void SpawnGrass()
    {

    int halfWidth = width / 2;
    int halfHeight = height / 2;

        for (int x = -halfWidth; x < halfWidth; x++)
        {
            for (int y = -halfHeight; y < halfHeight; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector3 worldPos = tilemap.GetCellCenterWorld(cellPosition);

                GameObject grass = Instantiate(grassPrefab, worldPos, Quaternion.identity, transform);
                grass.tag = "Grass"; // ����������� ���������� ���

                Debug.Log($"����� ����� � {cellPosition}, ���: {grass.tag}");
            }
        }
    }
}
