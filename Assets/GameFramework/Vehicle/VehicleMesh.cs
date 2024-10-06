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

    void Update()
    {
        if (vehicle)
        {
            Vector3 localPos = meshOffset;

            if(vehicle.aeroState == VehicleAeroState.Jumping)
            {
                float d = Mathf.Clamp01((Time.time - vehicle.jumpStartTime) / vehicle.jumpDuration);
                d = 1 - Mathf.Abs(d - 0.5f) * 2;

                localPos += Vector3.up * d * 0.1f;
            }

            transform.position = vehicle.vehicleBox.transform.TransformPoint(localPos);

            Quaternion targetRotaton = vehicle.drifting
                ? Quaternion.AngleAxis(vehicle.driftYaw * 30.0f, vehicle.vehicleBox.transform.up) * vehicle.vehicleBox.transform.rotation
                : Quaternion.AngleAxis(vehicle.steerValue * 5.0f, vehicle.vehicleBox.transform.up) * vehicle.vehicleBox.transform.rotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotaton, Time.deltaTime * lerpRate);
        }

    }

}