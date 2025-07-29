using UnityEngine;


public enum HouseClass
{
    Peasant,
    Worker,
    Engineer
}

public class House : MonoBehaviour
{
    public HouseClass houseClass;

    public HouseClass currentClass = HouseClass.Peasant; //хранит тип жителя

    public int capacity = 10; //вместимость здания
    public int currentCitizens = 0; //население в начале игры

    public GameObject upgradedPrefab; //улучшенная версия дома

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
        //заселяем, но не привышаем лимит дома
        currentCitizens  = Mathf.Min(currentCitizens  + amount, capacity);
        PopulationManager.Instance.UpdatePopulationCounts(); //обновляем общее состояние населения в игре
    }

    //метод для удаления жителей при сносе
    public void RemoveAllCitizens()
    {
        currentCitizens  = 0; //становится 0
        PopulationManager.Instance.UpdatePopulationCounts(); //обновляем
    }

    //метод для вычисления комфорта
    public void CalculateComfortAndPopulate()
    {
        RemoveAllCitizens(); //очищаем дом от предыдущих жителей перед перерасчётом

        bool hasBar = false;
        bool hasMarket = false;

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
                        if (source.type == ComfortType.Bar) hasBar = true; //увеличиваем коморфт, если есть бар
                        if (source.type == ComfortType.Market) hasMarket = true; //увеличиваем комфорт, если есть рынок
                    }
                    PopulationManager.Instance.UpdatePopulationCounts();
                }
            }
        }

        //рассчитываем население
        int maxResidents = 5; //начинаем с 5
        if (hasBar) maxResidents += 2; //+2 за бар
        if (hasMarket) maxResidents += 3; //+3 за рынок

        //заселяем дом максимально возможным количеством жителей с учётом комфорта
        AddCitizens(maxResidents);
        PopulationManager.Instance.UpdatePopulationCounts();
    }

    //улучшение здания
    public void TryUpgrade()
    {
        //проверка, что жителей больше 10
        if (currentCitizens < 10)
        {
            Debug.Log("Недостаточно жителей для улучшения!");
            return;
        }

        // Проверка: хватает ли ресурсов (10 камня)?
        //if (! ResourceStorage.Instance.HasEnough(ResourceType.Stone, 10))
        //{
        //    Debug.Log("Недостаточно камня!");
        //    return;
        //}

        //забираем ресурсы и производим апгрейд
        //ResourceStorage.Instance.SpendResource(ResourceType.Stone, 10);

        //создаём новое здание
        GameObject upgraded = Instantiate(upgradedPrefab, transform.position, Quaternion.identity);

        //обработка нового дома
        House upgradedHouse = upgraded.GetComponent<House>();
        if (upgradedHouse != null)
        {
            upgradedHouse.PlaceHouse(); //заселение и комфорт
            upgradedHouse.currentClass = GetNextClass(currentClass); //переход к следующему классу
        }

        //удаляем старое
        Destroy(gameObject);
        RemoveAllCitizens();
        Debug.Log("Дом улучшен!");
    }

    //переход к следующему классу
    private HouseClass GetNextClass(HouseClass current)
    {
        switch (current)
        {
            case HouseClass.Peasant: return HouseClass.Worker;
            case HouseClass.Worker: return HouseClass.Engineer;
            default: return current;
        }
    }

    //выбор здания через метод IsBoostingMode
    private void OnMouseDown()
    {
        if (BoostingManager.Instance.IsBoostingMode())
        {
            TryUpgrade();
            //BoostingManager.Instance.ToggleBoostingMode(); //отключаем режим после одного улучшения
        }
    }
}
