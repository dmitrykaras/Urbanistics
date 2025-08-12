using UnityEngine;
using UnityEngine.EventSystems;
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

    public bool isBoostingMode = false; //флаг дл€ вкл/выкл режима аппгрейда
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
        //если мышка наведена на UI, то игнорировать ввод
        if (EventSystem.current.IsPointerOverGameObject()) return;

        BoostingGhost();
    }

    //вкл-выкл Boosting mode
    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode; //вкл/выкл
        Debug.Log("Boosting mode: " + (isBoostingMode ? "ON" : "OFF"));
        UpdateBoostingButtonColor();

        if (RoadPainter.Instance.isPainting) RoadPainter.Instance.DisableRoadMode();
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();
    }

    //обновлени€ цвета кнопки BoostingButton
    public void UpdateBoostingButtonColor()
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

    //выключение режима улучшени€ зданий
    public void DisableBoostingMode()
    {
        if (isBoostingMode)
        {
            isBoostingMode = false;
            Debug.Log("BoostingMode: " + (isBoostingMode ? "ON" : "OFF"));
            UpdateBoostingButtonColor();
            BoostingGhostInstance.SetActive(false);
        }
    }
}
