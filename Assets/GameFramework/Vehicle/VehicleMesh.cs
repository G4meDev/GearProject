using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VehicleMesh : MonoBehaviour
{
    private Vehicle vehicle;

    public float lerpRate = 10.0f;

    void Start()
    {
        vehicle = transform.root.GetComponentInChildren<Vehicle>();
    }

    void Update()
    {
        if (vehicle)
        {
            Vector3 targetPosition = vehicle.vehicleProxy.position;
            Quaternion targetRotaton = vehicle.vehicleProxy.rotation;

            float lerpAlpha = Mathf.Clamp01(Time.deltaTime * lerpRate);

            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpAlpha);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotaton, lerpAlpha);
        }

    }

}