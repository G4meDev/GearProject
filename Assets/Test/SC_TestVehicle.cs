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

    public MeshRenderer boostIndicator;

    public Text speedText;
    public Text tractionText;
    public Text boostText;
    public Text reserveText;

    public float gravityStr = 25.0f;

    public float counterForceStr = 0.1f;

    public float maxSpeed = 20.0f;
    public float accel = 20.0f;

    public float rayDist = 1.0f;

    public float speedModifierIntensity;
    public float speedModifierReserveTime;

    public SpeedModifierData boostpadModifierData;

    public float boostIntensity = 10.0f;

    public float traction = 0.8f;
    public float driftTraction = 0.2f;
    public AnimationCurve steerCurve;

    public float groundDrag = 0.42f;
    public float airDrag = 0.62f;

    public float airSteerStr = 0.4f;

    public float jumpStr = 300.0f;

    [HideInInspector]
    public bool drifting = false;

    [HideInInspector]
    public bool boosting = false;

    [HideInInspector]
    public float boostAmount = 0.0f;

    [HideInInspector]
    public float forwardSpeed = 0.0f;

    float maxSpeedModifier = 20.0f;

    bool isBoosting()
    {
        return boostAmount > 0 || boosting;
    }

    float minAirbornTime = 0.5f;

    float lastTimeOnGround = 0.0f;
    bool airborn = false;

    [HideInInspector]
    float lastjumpTime = 0;

    float jumpTimeTreshold = 0.5f;

    [HideInInspector]
    public float vInput;

    [HideInInspector]
    public float hInput;

    void Start()
    {
        Application.targetFrameRate = 60;
        //Time.fixedDeltaTime = 0.01f;
    }

    public void ApplySpeedModifier(ref SpeedModifierData data)
    {
        speedModifierReserveTime += data.reserverTime;

        if (speedModifierIntensity < data.intensity)
        {
            speedModifierIntensity = data.intensity;
        }
    }

    public void IncreaseSpeedTo(float targetSpeed)
    {
        if (forwardSpeed < targetSpeed)
        {
            //vehicleProxy.velocity = (vehicleBox.transform.forward * maxSpeed) + (Vector3.up * vehicleProxy.velocity.y);
            vehicleProxy.velocity = (vehicleBox.transform.forward * maxSpeed);
        }
    }

    private void FixedUpdate()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

        bool jumping = UnityEngine.Input.GetButton("Jump");
        boosting = UnityEngine.Input.GetButton("Boost");

        speedModifierReserveTime -= Time.fixedDeltaTime;
        if (speedModifierReserveTime < 0)
        {
            speedModifierReserveTime = 0;
            speedModifierIntensity = 0;
        }

        float speedModifierAlpha = speedModifierIntensity / maxSpeedModifier;

        boostIndicator.GetComponent<MeshRenderer>().material.SetFloat("_a", speedModifierAlpha);

        boostText.text = string.Format("Boost : {0:F2}", speedModifierIntensity);
        reserveText.text = string.Format("Reserve : {0:F2}", speedModifierReserveTime);

        forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);

        if (true)
        {
            boostAmount = Mathf.Max(boostAmount - Time.fixedDeltaTime, 0.0f);
        }

        if (drifting && !jumping) 
        {
            drifting = false;
        }

        vehicleBox.transform.position = vehicleProxy.transform.position;

        RaycastHit hit;
        Ray ray = new Ray(vehicleProxy.transform.position, -Vector3.up);

        Debug.DrawLine(vehicleProxy.transform.position, vehicleProxy.transform.position - Vector3.up * rayDist);

        Vector3 boxUp = Vector3.up;

        Vector3 gravityDir = -Vector3.up;

        // TODO: make track surface and track wall layer
        LayerMask layerMask = LayerMask.GetMask("Default");
        bool bhit = Physics.Raycast(ray, out hit, rayDist, layerMask);
        if (!bhit)
        {
            

//             float steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime * airSteerStr;
//             steerValue = forwardSpeed > 0 ? steerValue : -steerValue;
// 
//             Vector3 xyVelocity = new Vector3(vehicleProxy.velocity.x, 0, vehicleProxy.velocity.z); 
// 
//             vehicleBox.transform.rotation = vehicleBox.transform.rotation * Quaternion.AngleAxis(steerValue, vehicleBox.transform.up);
//             Vector3 newForward = Vector3.Normalize(new Vector3(vehicleMesh.transform.forward.x, 0, vehicleMesh.transform.forward.z));
// 
//             Vector3 dirVelocity = newForward * xyVelocity.magnitude;
// 
//             vehicleProxy.velocity = dirVelocity + new Vector3(0, vehicleProxy.velocity.y, 0);
// 
//             vehicleProxy.drag = airDrag;
        }
        if (bhit)
        {
            vehicleProxy.drag = groundDrag;

            if (airborn && Time.time - lastTimeOnGround > 0.1f)
            {
                airborn = false;
                float d = Time.time - lastTimeOnGround;

                if (d > minAirbornTime)
                {
                    boostAmount += d;
                }
            }

            boxUp = hit.normal;


            float steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime;
            steerValue = forwardSpeed > 0 ? steerValue : -steerValue;

            vehicleBox.transform.rotation = vehicleBox.transform.rotation * Quaternion.AngleAxis( steerValue, vehicleBox.transform.up);

            float modifier = Mathf.Max(speedModifierIntensity, isBoosting() ? boostIntensity : 0);

            if (modifier > 0)
            {
                //IncreaseSpeedToMax();
            }

            float friction = hit.collider.material.dynamicFriction;

            float enginePower = Mathf.Abs(forwardSpeed) < (maxSpeed + modifier - friction) ? accel : 0;
            enginePower *= modifier > 0 ? 1 : vInput;

            vehicleProxy.AddForce(vehicleBox.transform.forward * enginePower, ForceMode.Acceleration);

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : slipingSpeed / vehicleProxy.velocity.magnitude;


            if (Mathf.Abs(slipingSpeedRatio) > 0)
            {
                float t = drifting ? driftTraction : traction;

                vehicleProxy.AddForce(-slipingSpeed * t * vehicleBox.transform.right, ForceMode.VelocityChange);

                tractionText.text = string.Format("Traction = {0:F2}", t);
            }

            if (jumping && !drifting && Time.time - lastjumpTime > jumpTimeTreshold)
            {
                vehicleProxy.AddForce(jumpStr * hit.normal, ForceMode.Acceleration);

                //drifting = true;

                lastjumpTime = Time.time;

                lastTimeOnGround = Time.time;

                airborn = true;
            }

            if (vInput != 0)
            {
                gravityDir = -hit.normal;
            }
        }

        Vector3 nForward = Vector3.Normalize(Vector3.Cross(vehicleBox.transform.right, boxUp));
        Quaternion q = Quaternion.LookRotation(nForward, boxUp);
        vehicleBox.transform.rotation = q;


        vehicleProxy.AddForce(gravityDir * gravityStr, ForceMode.Acceleration);

        vehicleProxy.AddForce((vehicleProxy.velocity.magnitude == 0 ? 0 : counterForceStr) * -vehicleProxy.velocity.normalized, ForceMode.VelocityChange);

        speedText.text = string.Format("Speed : {0:F2}", forwardSpeed);
    }

    void Update()
    {

    }

    private void LateUpdate()
    {
        
    }
}
