using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VehicleMesh : MonoBehaviour
{
    private Vehicle vehicle;

    public Vector3 meshOffset;
    public float lerpRate = 10.0f;

    void Start()
    {
        vehicle = transform.root.GetComponentInChildren<Vehicle>();
    }

    void FixedUpdate()
    {
        if (vehicle)
        {
            transform.position = vehicle.vehicleBox.transform.TransformPoint(meshOffset);

            Quaternion targetRotaton = vehicle.drifting
                ? Quaternion.AngleAxis(vehicle.driftYaw * 30.0f, vehicle.vehicleBox.transform.up) * vehicle.vehicleBox.transform.rotation
                : vehicle.vehicleBox.transform.rotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotaton, Time.fixedDeltaTime * lerpRate);
        }

    }

}