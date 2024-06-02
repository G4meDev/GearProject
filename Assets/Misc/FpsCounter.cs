using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    public Text textComponent;

    void Start()
    {
        
    }

    void Update()
    {
        textComponent.text = Mathf.Ceil(1.0f/Time.deltaTime).ToString();
    }
}
