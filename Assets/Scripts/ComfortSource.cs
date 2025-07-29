using UnityEngine;

public enum ComfortType //здания дающее комфорт
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

    private void OnDestroy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 20f);
        foreach (var col in colliders)
        {
            var house = col.GetComponent<House>();
            if (house != null)
            {
                PopulationManager.Instance.RecalculateAllHouses();
            }
        }
    }


}
