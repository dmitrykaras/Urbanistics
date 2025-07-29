using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance;

    //���� ���� ������� �� �������� - ���������
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); //���� ��� ���� ������, �� ������� ����
    }

    public List<House> allHouses = new List<House>(); //������ ���� �����

    public int peasants, workers, engineers; //����� ���-�� ������� �� �������

    public int totalPeasants = 999; //����� ���������� ��������
    public int usedPeasants = 0;

    public int FreePeasants => totalPeasants - usedPeasants;

    //����� ��� UI
    public TextMeshProUGUI peasantsText;
    public TextMeshProUGUI workersText;
    public TextMeshProUGUI engineersText;


    //����������� ������ ����
    public void RegisterHouse(House house)
    {
        allHouses.Add(house);
        UpdatePopulationCounts();
        TryActivateAllProducers();
    }

    //�������� ���������� ���-�� ������� �� �������
    public void UpdatePopulationCounts()
    {
        peasants = workers = engineers = 0; //��������

        foreach (var house in allHouses)
        {
            switch (house.currentClass)
            {
                case HouseClass.Peasant:
                    peasants += house.currentCitizens ;
                    break;
                case HouseClass.Worker:
                    workers += house.currentCitizens ;
                    break;
                case HouseClass.Engineer:
                    engineers += house.currentCitizens ;
                    break;
            }
        }

        UpdateUI(); //��������� UI
        TryActivateAllProducers(); //�������� ������������ ���������� ������
    }

    //���������� UI
    private void UpdateUI()
    {
        peasantsText.text = $"P: {peasants}";
        workersText.text = $"W: {workers}";
        engineersText.text = $"E: {engineers}";
    }

    //���������� ���������� ��������� ������� ���������� ������
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

        return false; //������������ ��������� ����� ������� ������
    }

    //���������� ����� ������� (���� �������� �����������)
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

    //�������� ��� ����������� ����� ������� ����
    public void TryActivateAllProducers()
    {
        var producers = FindObjectsByType<ResourceProducer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var producer in producers)
        {
            producer.TryActivate();
        }
    }

    //��������� ���������� �� ��������� ��������
    public bool TryUsePeasants(int count)
    {
        if (FreePeasants >= count)
        {
            usedPeasants += count;
            return true;
        }
        return false;
    }

    //����������� count ��������
    public void ReleasePeasants(int count)
    {
        usedPeasants -= count;
        if (usedPeasants < 0) usedPeasants = 0; //����������� ��� usedPeasants �� ������ ������ ����.
    }

}

