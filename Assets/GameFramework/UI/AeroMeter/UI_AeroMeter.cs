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
        image.material.SetFloat("_low", SceneManager.playerVehicle.lowJumpTime);
        image.material.SetFloat("_mid", SceneManager.playerVehicle.midJumpTime);
        image.material.SetFloat("_high", SceneManager.playerVehicle.highJumpTime);

        canvas.worldCamera = SceneManager.playerVehicle.GetComponentInChildren<VehicleCamera>().camera;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (SceneManager.playerVehicle)
        {
            image.material.SetFloat("_airborneTime", SceneManager.playerVehicle.airborneTime);
        }
    }
}
