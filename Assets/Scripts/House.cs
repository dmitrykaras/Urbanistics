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

        // Заселяем только если комфорт позволяет
        CalculateComfortAndPopulate();
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

    public void PlaceHouse()
    {
        isPlaced = true;
    }

    public void RemoveAllCitizens()
    {
        currentPopulation = 0;
        PopulationManager.Instance.UpdatePopulationCounts();
    }


    public void CalculateComfortAndPopulate()
    {
        int comfort = 0;

        // Обходим радиус 5 клеток вокруг дома
        Vector3Int centerCell = Builder.Instance.buildTilemap.WorldToCell(transform.position);

        for (int x = -5; x <= 5; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                Vector3 checkWorld = Builder.Instance.buildTilemap.GetCellCenterWorld(checkCell);

                Collider2D collider = Physics2D.OverlapPoint(checkWorld);
                if (collider != null)
                {
                    if (collider.CompareTag("Bar")) comfort++;
                    if (collider.CompareTag("Market")) comfort++;
                }
            }
        }

        // Рассчитываем максимальное возможное население
        int maxResidents = 5;
        if (comfort > 0)
            maxResidents += 2; // за бар
        if (comfort > 1)
            maxResidents += 3; // за рынок

        AddCitizens(maxResidents);
    }
}
