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

        //���� ������ ������ ��������� 3 �������� �� ��� ��������� �� ����
        if (freePeasants > totalPeasants)
            freePeasants = totalPeasants;
        if (freeWorkers > totalWorkers)
            freeWorkers = totalWorkers;
        if (freeEngineers > totalEngineers)
            freeEngineers = totalEngineers;

        UpdateUI();
    }

    //����������� ������ ���� � �������� ���������
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

    //���������� ������� 
    public void AddAndAssignFreeAndCurrent(House house)
    {
        freePeasants += house.currentCitizens;
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
