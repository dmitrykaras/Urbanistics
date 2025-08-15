using UnityEngine;
using UnityEngine.UI;

public class CursorMode : MonoBehaviour
{
    public static CursorMode Instance { get; private set; }

    public bool CursorModeRun = false;

    public Image cursorButtonImage;

    public Sprite cursorDefaultSprite;
    public Sprite cursorActiveSprite;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorMode();
        }
    }

    public void ToggleCursorMode()
    {
        CursorModeRun = !CursorModeRun;
        Debug.Log("CursorMode: " + (CursorModeRun ? "ON" : "OFF"));

        if (Builder.Instance.ghostInstance != null)
        {
            Builder.Instance.ghostInstance.SetActive(false); ; //удаляем предыдущий призрачный объект, если был
        }
        if (RoadPainter.Instance.RoadGhostInstance != null)
        {
            RoadPainter.Instance.RoadGhostInstance.SetActive(false);
        }
        if (Builder.Instance.bulldozerGhostInstance != null)
        {
            Builder.Instance.bulldozerGhostInstance.SetActive(false);
        }

        if (RoadPainter.Instance.isPainting) RoadPainter.Instance.DisableRoadMode();
        if (BoostingManager.Instance.isBoostingMode) BoostingManager.Instance.DisableBoostingMode();
        if (Builder.Instance.bulldozerMode) Builder.Instance.DisableBulldozerMode();

        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        if (cursorButtonImage != null)
            cursorButtonImage.sprite = CursorModeRun ? cursorActiveSprite : cursorDefaultSprite;
    }

    public void DisableCursorMode()
    {
        CursorModeRun = false;

    }
}
