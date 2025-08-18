using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum CitizenClass
{
    Peasant,   //крестьянин
    Worker,    //рабочий
    Engineer   //инженер
}

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //все дома для подсчёта населения
    public List<House> allHouses = new List<House>();

    [Header("Параметры населения")]
    public int totalPeasants, totalWorkers, totalEngineers; //общее население по классам

    public int freePeasants, freeWorkers, freeEngineers; //свободное население по классам

    public int workingPeasants, workingWorkers, workingEngineers;


    [Header("Текст в UI")]
    public TextMeshProUGUI peasantsText;
    public TextMeshProUGUI workersText;
    public TextMeshProUGUI engineersText;

    //пересчёт общего и свободного населения
    public void RecalculatePopulation()
    {
        totalPeasants = totalWorkers = totalEngineers = 0;

        foreach (var house in allHouses)
        {
            switch (house.currentClass)
            {
                case HouseClass.Peasant:
                    totalPeasants += house.currentCitizens;
                    break;
                case HouseClass.Worker:
                    totalWorkers += house.currentCitizens;
                    break;
                case HouseClass.Engineer:
                    totalEngineers += house.currentCitizens;
                    break;
            }
        }

        //либо вообще убрать следующие 3 проверки тк они расчитаны на баги
        if (freePeasants > totalPeasants)
            freePeasants = totalPeasants;
        if (freeWorkers > totalWorkers)
            freeWorkers = totalWorkers;
        if (freeEngineers > totalEngineers)
            freeEngineers = totalEngineers;

        UpdateUI();
    }

    //регистрация нового дома и пересчёт населения
    public void RegisterHouse(House house)
    {
        if (!allHouses.Contains(house))
        {
            DeactivateAllResourceProducers();
            allHouses.Add(house);
            AddAndAssignFreeAndCurrent(house);
            RecalculatePopulation();
        }
    }

    //удаление дома
    public void UnregisterHouse(House house)
    {
        if (allHouses.Contains(house))
        {
            allHouses.Remove(house);
            freePeasants -= house.currentCitizens;
            if (freePeasants < 0) freePeasants = 0;
            RecalculatePopulation();
        }
    }

    //добавление жителей 
    public void AddAndAssignFreeAndCurrent(House house)
    {
        freePeasants += house.currentCitizens;
    }

    //для всех добывающих зданий isActive = false (GENIUS SOLUTION)
    public void DeactivateAllResourceProducers()
    {
        var producers = Object.FindObjectsByType<ResourceProducer>(FindObjectsSortMode.None);

        foreach (var prod in producers)
        {
            if (prod.isActive)
            {
                prod.Deactivate();
            }
        }
    }

    //обновление UI
    private void UpdateUI()
    {
        if (peasantsText != null)
            peasantsText.text = $"Peasants: {totalPeasants} Free: {freePeasants}";
        if (workersText != null)
            workersText.text = $"Workers: {totalWorkers} Free: {freeWorkers}";
        if (engineersText != null)
            engineersText.text = $"Engineers: {totalEngineers} Free: {freeEngineers}";
    }

    //проверка, хватает ли свободных работников указанного класса
    public bool CanAssignWorkers(CitizenClass type, int amount)
    {
        return GetFreeWorkers(type) >= amount;
    }

    //получить количество свободных для класса
    public int GetFreeWorkers(CitizenClass type)
    {
        return type switch
        {
            CitizenClass.Peasant => freePeasants,
            CitizenClass.Worker => freeWorkers,
            CitizenClass.Engineer => freeEngineers,
            _ => 0
        };
    }

    //попытка назначить работников (забронировать)
    public bool TryAssignWorkers(CitizenClass type, int amount)
    {
        if (!CanAssignWorkers(type, amount))
            return false;

        switch (type)
        {
            case CitizenClass.Peasant:
                freePeasants -= amount;
                break;
            case CitizenClass.Worker:
                freeWorkers -= amount;
                break;
            case CitizenClass.Engineer:
                freeEngineers -= amount;
                break;
        }

        UpdateUI();
        return true;
    }

    //освободить работников (вернуть в свободные)
    public void ReleaseWorkers(CitizenClass type, int amount)
    {
        switch (type)
        {
            case CitizenClass.Peasant:
                freePeasants += amount;
                freePeasants = Mathf.Min(freePeasants, totalPeasants);
                break;
            case CitizenClass.Worker:
                freeWorkers += amount;
                freeWorkers = Mathf.Min(freeWorkers, totalWorkers);
                break;
            case CitizenClass.Engineer:
                freeEngineers += amount;
                freeEngineers = Mathf.Min(freeEngineers, totalEngineers);
                break;
        }
        UpdateUI();
    }
}
