using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    [Header("Ќастройки перетаскивани€")]
    public float dragSpeed = 1f; //скорость перемещени€

    private Vector3 dragOrigin; //исходна€ точка нажати€
    private bool isDragging = false;

    void Update()
    {
        //нажатие средней кнопки мыши
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        //отпустили кнопку
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        //перетаскивание
        if (isDragging)
        {
            Vector3 difference = Camera.main.ScreenToWorldPoint(dragOrigin) - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            difference.z = 0; //игнорируем ось Z

            transform.position += difference * dragSpeed;

            dragOrigin = Input.mousePosition; //обновл€ем исходную точку
        }
    }
}
