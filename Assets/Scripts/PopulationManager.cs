using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<House> allHouses = new List<House>();

    public int peasants, workers, engineers;

    public void RegisterHouse(House house)
    {
        allHouses.Add(house);
        UpdatePopulationCounts();
    }

    public void UpdatePopulationCounts()
    {
        peasants = workers = engineers = 0;

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

        UpdateUI();
    }

    private void UpdateUI()
    {
        //��������:
        Debug.Log($"���������: {peasants}, �������: {workers}, ��������: {engineers}");
    }

    public int GetFreeCitizens(CitizenClass citizenClass)
    {
        //� ���������� � ���� �������� ������������� �� ���������
        return citizenClass switch
        {
            CitizenClass.Peasant => peasants,
            CitizenClass.Worker => workers,
            CitizenClass.Engineer => engineers,
            _ => 0,
        };
    }

    //�������� �������� ������ ���������� ����� ���������� ������
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

        return false;
    }

    //���������� ����� �������
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

}
