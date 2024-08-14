using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class SC_TestVehicle : MonoBehaviour
{
    public Rigidbody vehicleProxy;
    public GameObject vehicleMesh;
    public Camera camera;

    public Text speedText;
    public Text tractionText;

    public Vector3 offset = Vector3.zero;

    public float enginePower = 20.0f;

    public AnimationCurve tractionCurve;
    public AnimationCurve steerCurve;

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

            float forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleMesh.transform.forward);

            float steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime;
            steerValue = forwardSpeed > 0 ? steerValue : -steerValue;

            vehicleMesh.transform.rotation = vehicleMesh.transform.rotation * Quaternion.AngleAxis( steerValue, vehicleMesh.transform.up);
            vehicleProxy.AddForce(vehicleMesh.transform.forward * vInput * enginePower, ForceMode.Acceleration);

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleMesh.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : slipingSpeed / vehicleProxy.velocity.magnitude;

            float traction = tractionCurve.Evaluate(Mathf.Abs(slipingSpeedRatio));


            if (Mathf.Abs(slipingSpeedRatio) > 0)
            {
                vehicleProxy.AddForce(-slipingSpeed * traction * vehicleMesh.transform.right, ForceMode.VelocityChange);

                tractionText.text = string.Format("Traction = {0:F2}", traction);
            }
        }

        camera.transform.position = vehicleMesh.transform.position + (vehicleMesh.transform.forward * -5) + (Vector3.up * 2);
        camera.transform.LookAt(vehicleMesh.transform);

        speedText.text = string.Format("Speed : {0:F2}", vehicleProxy.velocity.magnitude);
    }

    void Update()
    {

    }
}
