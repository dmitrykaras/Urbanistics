using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance; //только 1 такой объект

    //если есть инстанс не назначен - назначаем
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); //если уже есть другой, то удаляем этот
    }

    public List<House> allHouses = new List<House>(); //список всех домов

    public int peasants, workers, engineers; //общее кол-во жителей по классам

    public int totalPeasants = 999; //общее количество крестьян (можно поменять по классу)
    public int usedPeasants = 0;

    public int FreePeasants => totalPeasants - usedPeasants;

    //регистрация нового дома
    public void RegisterHouse(House house)
    {
        allHouses.Add(house);
        UpdatePopulationCounts();
        TryActivateAllProducers();
    }

    //пересчёт суммарного кол-ва жителей по классам
    public void UpdatePopulationCounts()
    {
        peasants = workers = engineers = 0; //обнуляем

        foreach (var house in allHouses)
        {
            switch (house.currentClass)
            {
                case CitizenClass.Peasant:
                    peasants += house.currentPopulation;
                    break;
                case CitizenClass.Worker:
                    workers += house.currentPopulation;
                    break;
                case CitizenClass.Engineer:
                    engineers += house.currentPopulation;
                    break;
            }
        }

        UpdateUI(); //обновляем UI
        TryActivateAllProducers(); //пытаемся активировать добываюшее здание
    }

    //обновление UI
    private void UpdateUI()
    {
        //например:
        Debug.Log($"Крестьяне: {peasants}, Рабочие: {workers}, Инженеры: {engineers}");
    }

    //возвращает количество свободных жителей указанного класса
    public int GetFreeCitizens(CitizenClass citizenClass)
    {
        //в дальнейшем — если появится распределение по занятости
        return citizenClass switch
        {
            CitizenClass.Peasant => peasants,
            CitizenClass.Worker => workers,
            CitizenClass.Engineer => engineers,
            _ => 0,
        };
    }

    //пытается выделить нужное количество людей указанного класса
    public bool TryAssignWorkers(CitizenClass type, int amount)
    {
        switch (type)
        {
            case CitizenClass.Peasant:
                if (peasants >= amount)
                {
                    peasants -= amount;
                    UpdateUI();
                    return true;
                }
                break;
            case CitizenClass.Worker:
                if (workers >= amount)
                {
                    workers -= amount;
                    UpdateUI();
                    return true;
                }
                break;
            case CitizenClass.Engineer:
                if (engineers >= amount)
                {
                    engineers -= amount;
                    UpdateUI();
                    return true;
                }
                break;
        }

        return false; //недостаточно свободных людей нужного класса
    }

    //возвращает людей обратно (если работник освободился)
    public void ReleaseWorkers(CitizenClass type, int amount)
    {
        switch (type)
        {
            case CitizenClass.Peasant:
                peasants += amount;
                break;
            case CitizenClass.Worker:
                workers += amount;
                break;
            case CitizenClass.Engineer:
                engineers += amount;
                break;
        }

        UpdateUI();
    }

    //функуция для обноружения новой рабочей силы
    public void TryActivateAllProducers()
    {
        var producers = FindObjectsByType<ResourceProducer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var producer in producers)
        {
            producer.TryActivate(); // или .ActivateIfPossible(), если метод называется иначе
        }
    }

    //
    public bool TryUsePeasants(int count)
    {
        if (FreePeasants >= count)
        {
            usedPeasants += count;
            return true;
        }
        return false;
    }

    //
    public void ReleasePeasants(int count)
    {
        usedPeasants -= count;
        if (usedPeasants < 0) usedPeasants = 0;
    }
}

