using UnityEngine;

public class ResourceProducer : MonoBehaviour //автоматическая добыча ресурсов на игровом объекте 
{
    public ResourceType resourceType; //тип ресурса
    public int amountPerCycle = 1; //сколько едениц ресурса производится за один цикл
    public float intervalSeconds = 1f; //интервал между циклами

    private float timer;

    private Builder builder;

    public CitizenClass requiredType = CitizenClass.Peasant;
    public int requiredPeople = 3;
    private bool isActive = false;

    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>(); //находит активный объект с компонентом builder на сцене

        isActive = PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople);
        if (!isActive)
        {
            Debug.Log("Недостаточно людей, здание не работает");
            enabled = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime; //увеличивает таймер
        if (timer >= intervalSeconds) //если прошло достаточно времени, то обнулить таймер
        {
            timer = 0f; 
            builder?.AddResource(resourceType, amountPerCycle); //если builder найден, добавляем ресурсы указанного типа
        }
    }


    private void OnDestroy()
    {
        if (isActive)
            PopulationManager.Instance.ReleaseWorkers(requiredType, requiredPeople);
    }
}
