using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LapCounter : MonoBehaviour
{
    public Text text;

    public void UpdateLapCounter()
    {
        if(SceneManager.Get().localVehicle)
        {
            text.text = SceneManager.Get().localVehicle.currentLap.ToString() + "/" + SceneManager.Get().lapCount;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
