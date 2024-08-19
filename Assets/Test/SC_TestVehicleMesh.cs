using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            ? Quaternion.AngleAxis(Vehicle.driftYaw * 50.0f, VehicleBox.transform.up) * VehicleBox.transform.rotation
            : VehicleBox.transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotaton, Time.fixedDeltaTime * lerpRate);
// 
//         Vector3 newForward = Vector3.Normalize(Vector3.Cross(transform.right, Vector3.up));
//         cameraTarget.transform.position = transform.position + newForward * 10;
    }

}