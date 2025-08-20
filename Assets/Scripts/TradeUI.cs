using UnityEngine;

public class TradeUI : MonoBehaviour
{
    public static TradeUI Instance;

    public AudioClip TradeSound;

    [SerializeField] private GameObject tradeUI; //панель торговли

    private void Awake()
    {
        Instance = this;
        if (tradeUI != null)
            tradeUI.SetActive(false);
    }

    public void OpenUIMarket()
    {
        if (tradeUI != null)
            tradeUI.SetActive(true);
    }

    public void CloseUIMarket()
    {
        if (tradeUI != null)
            tradeUI.SetActive(false);
    }

    //5 дерева = 1 камень
    public void TradeWoodForStone()
    {
        if (ResourceStorage.Instance.HasEnough(ResourceType.Wood, 5))
        {
            ResourceStorage.Instance.Consume(ResourceType.Wood, 5);
            ResourceStorage.Instance.Add(ResourceType.Stone, 1);
            Debug.Log("Обмен: 5 дерева → 1 камень");
            Builder.Instance.PlaySound(TradeSound);
        }
        else
        {
            Debug.Log("Недостаточно дерева!");
        }
    }

    //100 камня = 1 железо
    public void TradeStoneForIron()
    {
        if (ResourceStorage.Instance.HasEnough(ResourceType.Stone, 100))
        {
            ResourceStorage.Instance.Consume(ResourceType.Stone, 100);
            ResourceStorage.Instance.Add(ResourceType.Iron, 1);
            Debug.Log("Обмен: 100 камня → 1 железо");
            Builder.Instance.PlaySound(TradeSound);
        }
        else
        {
            Debug.Log("Недостаточно камня!");
        }
    }

    //20 камня = 100 дерева
    public void TradeStoneForWood()
    {
        if (ResourceStorage.Instance.HasEnough(ResourceType.Stone, 20))
        {
            ResourceStorage.Instance.Consume(ResourceType.Stone, 20);
            ResourceStorage.Instance.Add(ResourceType.Wood, 100);
            Debug.Log("Обмен: 20 камня → 100 дерева");
            Builder.Instance.PlaySound(TradeSound);
        }
        else
        {
            Debug.Log("Недостаточно камня!");
        }
    }
}
