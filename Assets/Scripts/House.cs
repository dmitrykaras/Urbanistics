using UnityEngine;

public class House : MonoBehaviour
{
    public int capacity = 10; //вместимость здания
    public CitizenClass currentClass = CitizenClass.Peasant; //хранит тип жителя
    public int currentPopulation = 0; //население в начале игры

    private bool isPlaced = false; //флаг, чтобы не заселять дом дважды

    //регестрируем дом при его создании
    private void Start()
    {
        PopulationManager.Instance.RegisterHouse(this); //регестрация дома
    }

    //метод вызывается, если объект на сцене
    public void PlaceHouse()
    {
        if (isPlaced) return; //не заселять повторно
        isPlaced = true;

        CalculateComfortAndPopulate(); // запуск расчёта комфорта и заселения
    }

    //метод для добавления жителей в дом
    public void AddCitizens(int amount)
    {
        //заселяем, но не привыаем лимит дома
        currentPopulation = Mathf.Min(currentPopulation + amount, capacity);
        PopulationManager.Instance.UpdatePopulationCounts(); //обновляем общее состояние населения в игре
    }

    //улучшение дома
    public void UpgradeHouse()
    {
        //был крестьянин - стал рабочий, был рабочий - стал инженер
        if (currentClass == CitizenClass.Peasant)
            currentClass = CitizenClass.Worker;
        else if (currentClass == CitizenClass.Worker)
            currentClass = CitizenClass.Engineer;

        PopulationManager.Instance.UpdatePopulationCounts(); //обновляем данные населения
    }

    //метод для удаления жителей при сносе
    public void RemoveAllCitizens()
    {
        currentPopulation = 0; //становится 0
        PopulationManager.Instance.UpdatePopulationCounts(); //обновляем
    }

    //метод для вычисления контроля
    public void CalculateComfortAndPopulate()
    {
        int comfort = 0; //переменная для посчёта комфорта

        //получаем центральную клетку, где стоит дом
        Vector3Int centerCell = Builder.Instance.buildTilemap.WorldToCell(transform.position);

        //обходим радиус 5 клеток
        for (int x = -5; x <= 5; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                Vector3 checkWorld = Builder.Instance.buildTilemap.GetCellCenterWorld(checkCell);

                Collider2D collider = Physics2D.OverlapPoint(checkWorld);
                if (collider != null) //проверяем наличиее коллайдера
                {
                    ComfortSource source = collider.GetComponent<ComfortSource>();
                    if (source != null) //проверяем наличиее компонента
                    {
                        if (source.type == ComfortType.Bar) comfort++; //увеличиваем коморфт, если есть бар
                        if (source.type == ComfortType.Market) comfort++; //увеличиваем комфорт, если есть рынок
                    }
                }
            }
        }

        // рассчитываем население
        int maxResidents = 5; //начинаем с 5
        if (comfort >= 1) maxResidents += 2; //+2 за бар
        if (comfort >= 2) maxResidents += 3; //+3 за рынок

        //аселяем дом максимально возможным количеством жителей с учётом комфорта
        AddCitizens(maxResidents);
    }
}
