using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boostpad : MonoBehaviour
{
    //TODO: make new channel for boostpad collision

    public SpeedModifierData data;

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.transform.root.GetComponent<Vehicle>();

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
