using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Position : MonoBehaviour
{
    public Text text;

    void Start()
    {
        
    }

    void Update()
    {
        if(SceneManager.playerVehicle)
        {
            text.text = SceneManager.playerVehicle.position.ToString();
        }
    }
}
