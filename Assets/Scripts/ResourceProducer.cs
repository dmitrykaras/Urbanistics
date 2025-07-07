using UnityEngine;

public class ResourceProducer : MonoBehaviour
{
    public ResourceType resourceType;
    public int amountPerCycle = 1;
    public float intervalSeconds = 1f;

    private float timer;

    private Builder builder;

    void Start()
    {
        builder = Object.FindFirstObjectByType<Builder>(); 
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= intervalSeconds)
        {
            timer = 0f;
            builder?.AddResource(resourceType, amountPerCycle);
        }
    }
}
