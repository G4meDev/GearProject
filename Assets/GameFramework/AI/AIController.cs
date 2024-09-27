using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    void Update()
    {
        if (vehicle)
        {
            vehicle.vInput = 1;
        }
    }


}