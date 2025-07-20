using UnityEngine;
using UnityEngine.U2D;

public class ResourceProducer : MonoBehaviour //автоматическая добыча ресурсов на игровом объекте 
{
    public ResourceType resourceType; //тип ресурса
    public int amountPerCycle = 1; //сколько едениц ресурса производится за один цикл
    public float intervalSeconds = 1f; //интервал между циклами

    private float timer;

    private Builder builder;

    public CitizenClass requiredType = CitizenClass.Peasant; //класс требуемый для работа
    public int requiredPeople = 3; //кол-во нужное для работы здания
    private bool isActive = false; //флаг, работает ли здание сейчас

    //метод при старте объекта
    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>(); //находит активный объект с компонентом builder на сцене

        //пытаемся назначить работников нужного класса и количества
        isActive = PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople);
        if (!isActive)
        {
            Debug.Log("Недостаточно людей, здание не работает");
            enabled = false; //отключаем Update, чтобы не тратить ресурсы
        }
    }

    //метод, который возвращает каждый кадр
    void Update()
    {
        timer += Time.deltaTime; //увеличивает таймер
        if (timer >= intervalSeconds) //если прошло достаточно времени, то обнулить таймер
        {
            timer = 0f; 
            builder?.AddResource(resourceType, amountPerCycle); //если builder найден, добавляем ресурсы указанного типа
        }
    }

    //метод вызывающийся при уничтожении здания (сносе)
    private void OnDestroy()
    {
        //если здание было активно, возвращаем работников обратно
        if (isActive)
            PopulationManager.Instance.ReleaseWorkers(requiredType, requiredPeople);
    }

    //метод для попытки активации здания
    public void TryActivate()
    {
        //если здание уже активно, ничего не делаем
        if (isActive)
            return;

        //пытаемся назначить работников и активировать здание
        if (PopulationManager.Instance.TryAssignWorkers(requiredType, requiredPeople))
        {
            isActive = true;
            Debug.Log($"{gameObject.name} начал работу: назначено {requiredPeople} {requiredType}");
            enabled = true; // если скрипт был отключен, включаем его
        }
    }

    //медот отключения здания
    public void Deactivate()
    {
        if (!isActive) return;

        PopulationManager.Instance.ReleasePeasants(requiredPeople);
        isActive = false;
        Debug.Log($"Здание {name} деактивировано, работники освобождены");
    }

}
