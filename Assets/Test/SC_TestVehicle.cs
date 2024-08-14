using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Windows;

public class SC_TestVehicle : MonoBehaviour
{
    public Rigidbody vehicleProxy;
    public GameObject vehicleMesh;
    public Camera camera;

    public Vector3 offset = Vector3.zero;

    public float enginePower = 20.0f;
    public float rotationRate = 200.0f;

    public float orientationLerppRate = 0.01f;

    [HideInInspector]
    public float vInput;

    [HideInInspector]
    public float hInput;

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void FixedUpdate()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

        vehicleMesh.transform.position = vehicleProxy.transform.position + offset;

        RaycastHit hit;
        Ray ray = new Ray(vehicleProxy.transform.position, -Vector3.up);

        bool bhit = Physics.Raycast(ray, out hit, vehicleProxy.transform.localScale.x * 1.5f);
        if (bhit)
        {
            Vector3 newForward = Vector3.Normalize(Vector3.Cross(vehicleMesh.transform.right, hit.normal));
            Quaternion q = Quaternion.LookRotation(newForward, hit.normal);
            vehicleMesh.transform.rotation = Quaternion.Slerp(vehicleMesh.transform.rotation, q, Mathf.Clamp01(Time.fixedTime * orientationLerppRate));

            vehicleMesh.transform.rotation = vehicleMesh.transform.rotation * Quaternion.AngleAxis(hInput * Time.fixedDeltaTime * rotationRate, vehicleMesh.transform.up);
            vehicleProxy.AddForce(vehicleMesh.transform.forward * vInput * enginePower, ForceMode.Acceleration);
        }

        camera.transform.position = vehicleMesh.transform.position + (vehicleMesh.transform.forward * -5) + (Vector3.up * 2);
        camera.transform.LookAt(vehicleMesh.transform);
    }

    void Update()
    {

    }
}
