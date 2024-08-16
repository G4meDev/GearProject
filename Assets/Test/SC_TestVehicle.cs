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

    public Material boostOffMaterial;
    public Material boostOnMaterial;

    public Text speedText;
    public Text tractionText;

    public Vector3 offset = Vector3.zero;

    public float rayDist = 1.0f;

    public SC_SpeedModifier boostpadModifier;

    public SpeedModifierData boostpadModifierData;

    public AnimationCurve engineCurve;
    public AnimationCurve boostCurve;

    public float traction = 0.8f;
    public float driftTraction = 0.2f;
    public AnimationCurve steerCurve;

    public float groundDrag = 0.42f;
    public float airDrag = 0.62f;

    public float airSteerStr = 0.4f;

    public Vector3 jumpStr = Vector3.zero;

    [HideInInspector]
    public bool drifting = false;

    [HideInInspector]
    public bool boosting = false;

    [HideInInspector]
    public float boostAmount = 0.0f;

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

    private void FixedUpdate()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

        bool jumping = UnityEngine.Input.GetButton("Jump");
        boosting = UnityEngine.Input.GetButton("Boost");

        if (boosting)
        {
            boostpadModifier.Register(boostpadModifierData);
        }

        Debug.Log(boostpadModifier.alive);
        Debug.Log(boostpadModifier.value);

        float forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);

        if (true)
        {
            boostAmount = Mathf.Max(boostAmount - Time.fixedDeltaTime, 0.0f);
        }

        if (isBoosting())
            boostIndicator.material = boostOnMaterial;
        else 
            boostIndicator.material = boostOffMaterial;

        if (drifting && !jumping) 
        {
            drifting = false;
        }

        vehicleBox.transform.position = vehicleProxy.transform.position;

        RaycastHit hit;
        Ray ray = new Ray(vehicleProxy.transform.position, -Vector3.up);

        Debug.DrawLine(vehicleProxy.transform.position, vehicleProxy.transform.position - Vector3.up * rayDist);

        Vector3 boxUp = Vector3.up;

        bool bhit = Physics.Raycast(ray, out hit, rayDist);
        if (!bhit)
        {
            

            float steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime * airSteerStr;
            steerValue = forwardSpeed > 0 ? steerValue : -steerValue;

            Vector3 xyVelocity = new Vector3(vehicleProxy.velocity.x, 0, vehicleProxy.velocity.z); 

            vehicleBox.transform.rotation = vehicleBox.transform.rotation * Quaternion.AngleAxis(steerValue, vehicleBox.transform.up);
            Vector3 newForward = Vector3.Normalize(new Vector3(vehicleMesh.transform.forward.x, 0, vehicleMesh.transform.forward.z));

            Vector3 dirVelocity = newForward * xyVelocity.magnitude;

            vehicleProxy.velocity = dirVelocity + new Vector3(0, vehicleProxy.velocity.y, 0);

            vehicleProxy.drag = airDrag;
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

            float enginePower = isBoosting() ? boostCurve.Evaluate(vehicleProxy.velocity.magnitude) : vInput * engineCurve.Evaluate(vehicleProxy.velocity.magnitude);

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
                vehicleProxy.AddForce(jumpStr, ForceMode.Acceleration);

                //drifting = true;

                lastjumpTime = Time.time;

                lastTimeOnGround = Time.time;

                airborn = true;
            }
        }

        Vector3 nForward = Vector3.Normalize(Vector3.Cross(vehicleBox.transform.right, boxUp));
        Quaternion q = Quaternion.LookRotation(nForward, boxUp);
        vehicleBox.transform.rotation = q;

        speedText.text = string.Format("Speed : {0:F2}", forwardSpeed);
    }

    void Update()
    {

    }

    private void LateUpdate()
    {
        
    }
}
