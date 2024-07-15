using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigVehicleSuspension : OverrideTransform
{
    [SerializeField]
    VehicleWheel wheel;

    void Start()
    {
        
    }


    void Update()
    {
        if (wheel) 
        {
            
        }
    }

}
