using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TestVehicleMesh : MonoBehaviour
{
    public GameObject VehicleBox;

    public Vector3 meshOffset;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = VehicleBox.transform.TransformPoint(meshOffset);
        transform.rotation = VehicleBox.transform.rotation;
    }
}
