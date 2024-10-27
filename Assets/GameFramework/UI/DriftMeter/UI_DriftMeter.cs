using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DriftMeter : MonoBehaviour
{
    public Image image;
    public Canvas canvas;

    public void OnPlayerChanged()
    {
        canvas.worldCamera = SceneManager.playerVehicle.GetComponentInChildren<VehicleCamera>().camera;
    }

    void Update()
    {
        if (SceneManager.playerVehicle)
        {
//             image.material.SetFloat("_Drifting", SceneManager.playerVehicle.isDrifting() ? 1 : 0);
//             image.material.SetFloat("_StartTime", SceneManager.playerVehicle.driftStartTime);
//             image.material.SetFloat("_Duration", SceneManager.playerVehicle.driftTimer);
//             image.material.SetFloat("_Counter", SceneManager.playerVehicle.driftCounter);
        }
    }
}
