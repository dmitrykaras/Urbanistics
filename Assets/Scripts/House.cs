using UnityEngine;

public class House : MonoBehaviour
{
    public int capacity = 10;
    public CitizenClass currentClass = CitizenClass.Peasant;
    public int currentPopulation = 0;

    private bool isPlaced = false;

    private void Start()
    {
        PopulationManager.Instance.RegisterHouse(this);
    }

    public void PlaceHouse()
    {
        if (isPlaced) return; // не заселять повторно
        isPlaced = true;

        CalculateComfortAndPopulate(); // запуск расчёта комфорта и заселения
    }

    public void AddCitizens(int amount)
    {
        currentPopulation = Mathf.Min(currentPopulation + amount, capacity);
        PopulationManager.Instance.UpdatePopulationCounts();
    }

    public void UpgradeHouse()
    {
        if (currentClass == CitizenClass.Peasant)
            currentClass = CitizenClass.Worker;
        else if (currentClass == CitizenClass.Worker)
            currentClass = CitizenClass.Engineer;

        PopulationManager.Instance.UpdatePopulationCounts();
    }

    public void RemoveAllCitizens()
    {
        currentPopulation = 0;
        PopulationManager.Instance.UpdatePopulationCounts();
    }

    public void CalculateComfortAndPopulate()
    {
        int comfort = 0;

        // получаем центральную клетку, где стоит дом
        Vector3Int centerCell = Builder.Instance.buildTilemap.WorldToCell(transform.position);

        // обходим радиус 5 клеток
        for (int x = -5; x <= 5; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                Vector3 checkWorld = Builder.Instance.buildTilemap.GetCellCenterWorld(checkCell);

                Collider2D collider = Physics2D.OverlapPoint(checkWorld);
                if (collider != null)
                {
                    ComfortSource source = collider.GetComponent<ComfortSource>();
                    if (source != null)
                    {
                        if (source.type == ComfortType.Bar) comfort++;
                        if (source.type == ComfortType.Market) comfort++;
                    }
                }
            }
        }

        // рассчитываем население
        int maxResidents = 5; // базово
        if (comfort >= 1) maxResidents += 2; // +2 за бар
        if (comfort >= 2) maxResidents += 3; // +3 за рынок

        AddCitizens(maxResidents);
    }
}
