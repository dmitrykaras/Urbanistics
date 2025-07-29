using UnityEngine;

public enum ComfortType //������ ������ �������
{
    Bar,
    Market
}

public class ComfortSource : MonoBehaviour
{
    public ComfortType type;

    private void Start()
    {
        PopulationManager.Instance.RecalculateAllHouses();
    }

}
