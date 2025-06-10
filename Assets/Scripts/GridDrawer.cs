using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GridRenderer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Color lineColor = new Color(1, 1, 1, 0.25f); // светлая полупрозрачная сетка
    private Material lineMaterial;

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Простой шейдер для линий
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnPostRender()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadOrtho(); // Рисовать в экранных координатах

        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        float stepX = 1f / (width * 1f);
        float stepY = 1f / (height * 1f);

        // Вертикальные линии
        for (int x = 0; x <= width; x++)
        {
            float xPos = x * stepX;
            GL.Vertex(new Vector3(xPos, 0, 0));
            GL.Vertex(new Vector3(xPos, 1, 0));
        }

        // Горизонтальные линии
        for (int y = 0; y <= height; y++)
        {
            float yPos = y * stepY;
            GL.Vertex(new Vector3(0, yPos, 0));
            GL.Vertex(new Vector3(1, yPos, 0));
        }

        GL.End();
        GL.PopMatrix();
    }
}
