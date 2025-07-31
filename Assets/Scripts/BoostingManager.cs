using UnityEngine;
using UnityEngine.UI;

public class BoostingManager : MonoBehaviour
{
    public static BoostingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private bool isBoostingMode = false; //флаг дл€ вкл/выкл режима аппгрейда
    public bool runningBoostingMode = false;

    //кнопки и цвета
    private Image BoostingButtonImage;
    public Button BoostingButton;
    public GameObject BoostingGhostInstance;

    void Start()
    {
        GameObject ghost = Resources.Load<GameObject>("BoostingGhost");
        if (ghost != null)
            BoostingGhostInstance = Instantiate(ghost);
    }

    void Update()
    {
        BoostingGhost();
    }

    //вкл-выкл Boosting mode
    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode; //вкл/выкл
        if (isBoostingMode)
        {
            runningBoostingMode = true;
        }
        else
        {
            runningBoostingMode = false;
        }
            Debug.Log("Boosting mode: " + (isBoostingMode ? "ON" : "OFF"));
        UpdateBoostingButtonColor();
    }

    //обновлени€ цвета кнопки BoostingButton
    private void UpdateBoostingButtonColor()
    {
        //если ссылка на компонент Image ещЄ не установлена Ч ищем еЄ на кнопке
        if (BoostingButtonImage == null)
            BoostingButtonImage = BoostingButton.GetComponent<Image>();
        //задаЄм цвет кнопки в зависимости от состо€ни€
        BoostingButtonImage.color = isBoostingMode ? new Color(0.4f, 0.8f, 0.4f, 1f) : Color.white;
    }

    //предоставл€ет доступ к этой переменной из других классов
    public bool IsBoostingMode()
    {
        return isBoostingMode;
    }

    //активаци€ BoostingGhost
    public void BoostingGhost()
    {
        if (isBoostingMode)
        {
            Builder.Instance.DestroyGhost();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //преобразуем в координаты клетки тайлмапа
            Vector3Int cellPosition = Builder.Instance.buildTilemap.WorldToCell(mouseWorldPos);

            //получаем центр клетки в мировых координатах
            Vector3 placePosition = Builder.Instance.buildTilemap.GetCellCenterWorld(cellPosition);

            if (BoostingGhostInstance != null)
            {
                BoostingGhostInstance.transform.position = placePosition;
                BoostingGhostInstance.SetActive(true);
            }
        }
        else
        {
            if (BoostingGhostInstance != null)
            {
                BoostingGhostInstance.SetActive(false);
            }
        }
    }

}
