using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SC_TestVehicleMesh : MonoBehaviour
{
    public SC_TestVehicle Vehicle;
    public GameObject VehicleBox;
    public GameObject cameraTarget;

    public Vector3 meshOffset;

    public float lerpRate = 10.0f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        transform.position = VehicleBox.transform.TransformPoint(meshOffset);

        Quaternion targetRotaton = Vehicle.drifting
            ? Quaternion.AngleAxis(Vehicle.driftYaw * 20.0f, VehicleBox.transform.up) * VehicleBox.transform.rotation
            : VehicleBox.transform.rotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotaton, Time.fixedDeltaTime * lerpRate);


        cameraTarget.transform.position = VehicleBox.transform.TransformPoint(meshOffset);
        cameraTarget.transform.rotation = Quaternion.Lerp(cameraTarget.transform.rotation, VehicleBox.transform.rotation, Time.fixedDeltaTime * lerpRate);
    }

}