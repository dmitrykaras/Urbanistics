using UnityEngine;

public class Market : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (CursorMode.Instance.CursorModeRun)
        {
            TradeUI.Instance.OpenUIMarket();
        }
        else
        {
            return;
        }
    }
}
