using System.Collections.Generic;
using System.Resources;
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

    public int currentPopulation;

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
                    PopulationManager.Instance.UpdatePopulationCounts();
                }
            }
        }

        // рассчитываем население
        int maxResidents = 5; //начинаем с 5
        if (comfort >= 1) maxResidents += 2; //+2 за бар
        if (comfort >= 2) maxResidents += 3; //+3 за рынок

        //заселяем дом максимально возможным количеством жителей с учётом комфорта
        AddCitizens(maxResidents);
        PopulationManager.Instance.UpdatePopulationCounts();
    }

    //улучшение здания
    public void TryUpgrade()
    {
        // Проверка: достаточно ли жителей?
        if (currentPopulation < 10)
        {
            Debug.Log("Недостаточно жителей для улучшения!");
            return;
        }

        // Проверка: хватает ли ресурсов (10 камня)?
        if (! ResourceStorage.Instance.HasEnough(ResourceType.Stone, 10))
        {
            Debug.Log("Недостаточно камня!");
            return;
        }

        // Забираем ресурсы и производим апгрейд
        ResourceStorage.Instance.SpendResource(ResourceType.Stone, 10);

        // Создаём новое здание
        GameObject upgraded = Instantiate(upgradedPrefab, transform.position, Quaternion.identity);

        // Удаляем старое
        Destroy(gameObject);
        Debug.Log("Дом улучшен!");
    }

    private void Upgrade(GameObject newPrefab, HouseClass newClass)
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        GameObject upgradedHouse = Instantiate(newPrefab, position, rotation);

        Destroy(gameObject); // Удаляем старый дом
    }

    private void OnMouseDown()
    {
        if (BoostingManager.Instance.IsBoostingMode())
        {
            TryUpgrade();
            BoostingManager.Instance.ToggleBoostingMode(); //отключаем режим после одного улучшения
        }
    }
}
