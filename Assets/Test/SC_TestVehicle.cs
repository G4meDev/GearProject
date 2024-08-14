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
    public GameObject vehicleBox;
    public GameObject vehicleMesh;

    public Text speedText;
    public Text tractionText;

    public Vector3 offset = Vector3.zero;

    public AnimationCurve engineCurve;

    public AnimationCurve tractionCurve;
    public AnimationCurve steerCurve;

    public float airSteerStr = 0.4f;

    public float orientationLerppRate = 0.01f;

    [HideInInspector]
    public float vInput;

    [HideInInspector]
    public float hInput;

    void Start()
    {
        Application.targetFrameRate = 60;
        //Time.fixedDeltaTime = 0.01f;
    }

    private void FixedUpdate()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

        vehicleBox.transform.position = vehicleProxy.transform.position;

        RaycastHit hit;
        Ray ray = new Ray(vehicleProxy.transform.position, -Vector3.up);

        bool bhit = Physics.Raycast(ray, out hit, vehicleProxy.transform.localScale.x * 1.2f);
        if (!bhit)
        {
            float forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);

            float steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime * airSteerStr;
            steerValue = forwardSpeed > 0 ? steerValue : -steerValue;

            vehicleBox.transform.rotation = vehicleBox.transform.rotation * Quaternion.AngleAxis(steerValue, vehicleBox.transform.up);
            Vector3 newForward = Vector3.Normalize(new Vector3(vehicleMesh.transform.forward.x, 0, vehicleMesh.transform.forward.z));

            Vector3 xyVelocity = new Vector3(vehicleProxy.velocity.x, 0, vehicleProxy.velocity.z);
            Vector3 dirVelocity = newForward * xyVelocity.magnitude;

            vehicleProxy.velocity = dirVelocity + new Vector3(0, vehicleProxy.velocity.y, 0);
        }
        if (bhit)
        {
            Vector3 newForward = Vector3.Normalize(Vector3.Cross(vehicleBox.transform.right, hit.normal));
            Quaternion q = Quaternion.LookRotation(newForward, hit.normal);
            vehicleBox.transform.rotation = q;

            float forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);

            float steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime;
            steerValue = forwardSpeed > 0 ? steerValue : -steerValue;

            vehicleBox.transform.rotation = vehicleBox.transform.rotation * Quaternion.AngleAxis( steerValue, vehicleBox.transform.up);

            float enginePower = engineCurve.Evaluate(vehicleProxy.velocity.magnitude);
            vehicleProxy.AddForce(vehicleBox.transform.forward * vInput * enginePower, ForceMode.Acceleration);

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : slipingSpeed / vehicleProxy.velocity.magnitude;

            float traction = tractionCurve.Evaluate(Mathf.Abs(slipingSpeedRatio));


            if (Mathf.Abs(slipingSpeedRatio) > 0)
            {
                vehicleProxy.AddForce(-slipingSpeed * traction * vehicleBox.transform.right, ForceMode.VelocityChange);

                tractionText.text = string.Format("Traction = {0:F2}", traction);
            }
        }

        speedText.text = string.Format("Speed : {0:F2}", vehicleProxy.velocity.magnitude);
    }

    void Update()
    {

    }

    private void LateUpdate()
    {
        
    }
}
