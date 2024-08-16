using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boostpad : MonoBehaviour
{
    //TODO: make new channel for boostpad collision

    public SpeedModifierData data;

    private void OnTriggerEnter(Collider other)
    {
        SC_TestVehicle vehicle = other.transform.root.GetComponent<SC_TestVehicle>();

        if (vehicle)
        {
            vehicle.ApplySpeedModifier(ref data);
        }

        else
        {
            Debug.LogError("boospad triggered by something t hat is not vehicle!");
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
