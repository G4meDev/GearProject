using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AeroMeter : MonoBehaviour
{
    Vehicle vehicle;

    public Image image;
    public Canvas canvas;
    
    public void InitVehicle(Vehicle inVehicle)
    {
        vehicle = inVehicle;

        image.material.SetFloat("_low", vehicle.lowJumpTime);
        image.material.SetFloat("_mid", vehicle.midJumpTime);
        image.material.SetFloat("_high", vehicle.highJumpTime);

        canvas.worldCamera = vehicle.GetComponentInChildren<VehicleCamera>().camera;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (vehicle)
        {
            image.material.SetFloat("_airborneTime", vehicle.airborneTime);
        }
    }
}
