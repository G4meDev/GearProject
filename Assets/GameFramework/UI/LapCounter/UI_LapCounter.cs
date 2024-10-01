using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LapCounter : MonoBehaviour
{
    public Text text;

    public void UpdateLapCounter()
    {
        if(SceneManager.playerVehicle)
        {
            text.text = SceneManager.playerVehicle.currentLap.ToString() + "/" + SceneManager.lapCount;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
