using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void OnAnyUIButtonClicked()
    {
        Builder.Instance?.DisableBulldozerMode();
        RoadPainter.Instance?.DisableRoadMode();
        BoostingManager.Instance?.DisableBoostingMode();
        CursorMode.Instance?.DisableCursorMode();
    }
}
