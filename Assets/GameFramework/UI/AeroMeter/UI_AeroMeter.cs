using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AeroMeter : MonoBehaviour
{
    public Image image;
    public Canvas canvas;
    
    public void OnPlayerChanged()
    {
        image.material.SetFloat("_low", SceneManager.Get().playerVehicle.lowJumpTime);
        image.material.SetFloat("_mid", SceneManager.Get().playerVehicle.midJumpTime);
        image.material.SetFloat("_high", SceneManager.Get().playerVehicle.highJumpTime);

        canvas.worldCamera = SceneManager.Get().playerVehicle.GetComponentInChildren<VehicleCamera>().camera;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (SceneManager.Get().playerVehicle)
        {
            image.material.SetFloat("_airborneTime", SceneManager.Get().playerVehicle.airborneTime);
        }
    }
}
