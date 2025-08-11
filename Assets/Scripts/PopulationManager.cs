using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum CitizenClass
{
    Peasant,   //����������
    Worker,    //�������
    Engineer   //�������
}

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //��� ���� ��� �������� ���������
    public List<House> allHouses = new List<House>();

    [Header("��������� ���������")]
    public int totalPeasants, totalWorkers, totalEngineers; //����� ��������� �� �������

    public int freePeasants, freeWorkers, freeEngineers; //��������� ��������� �� �������

    public int workingPeasants, workingWorkers, workingEngineers;


    [Header("����� � UI")]
    public TextMeshProUGUI peasantsText;
    public TextMeshProUGUI workersText;
    public TextMeshProUGUI engineersText;

    //�������� ������ � ���������� ���������
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

        if (freePeasants > totalPeasants || freePeasants == 0)
            freePeasants = totalPeasants;
        if (freeWorkers > totalWorkers || freeWorkers == 0)
            freeWorkers = totalWorkers;
        if (freeEngineers > totalEngineers || freeEngineers == 0)
            freeEngineers = totalEngineers;

        UpdateUI();
    }

    //����������� ������ ���� � �������� ���������
    public void RegisterHouse(House house)
    {
        if (!allHouses.Contains(house))
        {
            allHouses.Add(house);
            freePeasants += house.currentCitizens;
            RecalculatePopulation();
        }
    }

    //�������� ����
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

    //��� ���� ���������� ������ isActive = false (GENIUS SOLUTION)
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

    //���� ������ ������� ������ ����������, ��� ���� � ������, �� ��������� ������, ����� �������������� ��������
    private void CheckWorkerOverflow(CitizenClass type, ref int working, int total)
    {
        if (working > total)
        {
            int overflow = working - total;
            working = total; // ��������� �������� ���������
            NotifyBuildingsWorkersLost(type, overflow);
        }
    }

    //����������, ���� ���-�� ��������� ��������� �� �����-�� �������
    private void NotifyBuildingsWorkersLost(CitizenClass type, int lostCount)
    {
        foreach (var prod in FindObjectsByType<ResourceProducer>(FindObjectsSortMode.None))
        {
            if (prod.requiredType == type && prod.isActive)
            {
                prod.ForceDeactivate();
                lostCount -= prod.requiredPeople;
                if (lostCount <= 0) break;
            }
        }
    }


    //���������� UI
    private void UpdateUI()
    {
        if (peasantsText != null)
            peasantsText.text = $"Peasants: {totalPeasants} Free: {freePeasants}";
        if (workersText != null)
            workersText.text = $"Workers: {totalWorkers} Free: {freeWorkers}";
        if (engineersText != null)
            engineersText.text = $"Engineers: {totalEngineers} Free: {freeEngineers}";
    }

    //��������, ������� �� ��������� ���������� ���������� ������
    public bool CanAssignWorkers(CitizenClass type, int amount)
    {
        return GetFreeWorkers(type) >= amount;
    }

    //�������� ���������� ��������� ��� ������
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

    //������� ��������� ���������� (�������������)
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

    //���������� ���������� (������� � ���������)
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
