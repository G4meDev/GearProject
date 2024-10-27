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
        if(SceneManager.Get().playerVehicle)
        {
            text.text = SceneManager.Get().playerVehicle.position.ToString();
        }
    }
}
