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

    //кнопки и цвета
    private Image BoostingButtonImage;
    public Button BoostingButton;
    public GameObject BoostingGhostInstance;

    private void Start()
    {
        GameObject ghost = Resources.Load<GameObject>("BoostingGhost");
        if (ghost != null)
            BoostingGhostInstance = Instantiate(ghost);
    }

    public void ToggleBoostingMode()
    {
        isBoostingMode = !isBoostingMode; //вкл/выкл
        Debug.Log("Boosting mode: " + isBoostingMode);
        UpdateBoostingButtonColor();
    }

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

            //    if (BoostingManager.Instance.BoostingGhostInstance != null)
            //{
            //    BoostingManager.Instance.BoostingGhostInstance.transform.position = placePosition;
            //    BoostingManager.Instance.BoostingGhostInstance.SetActive(true);
            //}
}
