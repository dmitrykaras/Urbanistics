using UnityEngine;
using UnityEngine.U2D;

public class ResourceProducer : MonoBehaviour //�������������� ������ �������� �� ������� ������� 
{
    public ResourceType resourceType; //��� �������
    public int amountPerCycle = 1; //������� ������ ������� ������������ �� ���� ����
    public float intervalSeconds = 1f; //�������� ����� �������

    private float timer;

    private Builder builder;

    public CitizenClass requiredType = CitizenClass.Peasant; //����� ��������� ��� ������
    public int requiredPeople = 3; //���-�� ������ ��� ������ ������
    private bool isActive = false; //����, �������� �� ������ ������

    //����� ��� ������ �������
    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>(); //������� �������� ������ � ����������� builder �� �����

        //�������� ��������� ���������� ������� ������ � ����������
        isActive = PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople);
        if (!isActive)
        {
            Debug.Log("������������ �����, ������ �� ��������");
            enabled = false; //��������� Update, ����� �� ������� �������
        }
    }

    //�����, ������� ���������� ������ ����
    void Update()
    {
        timer += Time.deltaTime; //����������� ������
        if (timer >= intervalSeconds) //���� ������ ���������� �������, �� �������� ������
        {
            timer = 0f; 
            builder?.AddResource(resourceType, amountPerCycle); //���� builder ������, ��������� ������� ���������� ����
        }
    }

    //����� ������������ ��� ����������� ������ (�����)
    private void OnDestroy()
    {
        //���� ������ ���� �������, ���������� ���������� �������
        if (isActive)
            PopulationManager.Instance.ReleaseWorkers(requiredType, requiredPeople);
    }

    //����� ��� ������� ��������� ������
    public void TryActivate()
    {
        //���� ������ ��� �������, ������ �� ������
        if (isActive)
            return;

        //�������� ��������� ���������� � ������������ ������
        if (PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople))
        {
            isActive = true;
            Debug.Log($"{gameObject.name} ����� ������: ��������� {requiredPeople} {requiredType}");
            enabled = true; // ���� ������ ��� ��������, �������� ���
        }
    }

    //����� ���������� ������
    public void Deactivate()
    {
        if (!isActive) return;

        PopulationManager.Instance.ReleasePeasants(requiredPeople);
        isActive = false;
        Debug.Log($"������ {name} ��������������, ��������� �����������");
    }

}
