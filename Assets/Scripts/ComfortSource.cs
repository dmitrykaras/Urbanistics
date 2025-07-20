using UnityEngine;

public enum ComfortType //здания дающее комфорт
{
    Bar,
    Market
}

public class ComfortSource : MonoBehaviour
{
    public ComfortType type;
}
