using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public enum VehicleAeroState
{
    
    OnGround,
    Coyote,
    Jumping,
    Falling
}

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

    public float counterForceStr = 0.03f;

    public float maxSpeed = 20.0f;
    public float accel = 20.0f;

    public float rayDist = 1.0f;

    public float speedModifierIntensity;
    public float speedModifierReserveTime;

    public float boostIntensity = 10.0f;

    public float traction = 1.0f;
    public float driftTraction = 0.0f;
    public AnimationCurve steerCurve;
    public float steerVelocityFriction = 2.0f;

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


    float lastTimeOnGround = 0.0f;

    public float coyoteTime = 0.1f;

    public float jumpDelayTime = 0.1f;

    [HideInInspector]
    float lastjumpTime = 0;

    [HideInInspector]
    public float vInput;

    [HideInInspector]
    public float hInput;


    [HideInInspector]
    private RaycastHit hit;

    [HideInInspector]
    private bool bHit;

    [HideInInspector]
    private float steerValue;

    [HideInInspector]
    private float maxSpeedWithModifier;

    [HideInInspector]
    VehicleAeroState aeroState = VehicleAeroState.OnGround;

    bool isBoosting()
    {
        return boostAmount > 0 || boosting;
    }

    private bool CanJump()
    {
        return aeroState == VehicleAeroState.OnGround || aeroState == VehicleAeroState.Coyote;
    }

    private void OnStartJump()
    {
        if (CanJump())
        {
            vehicleProxy.AddForce(jumpStr * hit.normal, ForceMode.Acceleration);

            aeroState = VehicleAeroState.Jumping;

            lastjumpTime = Time.time;
        }
    }

    private void OnEndJump()
    {
        float airborneTime = Time.time - lastjumpTime;

        Debug.Log(airborneTime);
    }

    private float GetContactSurfaceFriction()
    {
        if (bHit)
        {
            return hit.collider.material.dynamicFriction;
        }

        return 0;
    }

    void UpdateAeroState()
    {
        if (!bHit)
        {
            if (aeroState != VehicleAeroState.Jumping)
            {
                if (Time.time < lastTimeOnGround + coyoteTime)
                {
                    aeroState = VehicleAeroState.Coyote;
                }
                else
                {
                    aeroState = VehicleAeroState.Falling;
                }
            }

        }

        else
        {
            if (aeroState == VehicleAeroState.Jumping)
            {
                // add delay to let ray get out of surface (ray is longer than vehicle proxy radius)
                if (Time.time > lastjumpTime + jumpDelayTime)
                {
                    aeroState = VehicleAeroState.OnGround;
                    lastTimeOnGround = Time.time;

                    OnEndJump();
                }
            }

            else
            {
                aeroState = VehicleAeroState.OnGround;
                lastTimeOnGround = Time.time;
            }
        }
    }

    public float GetMaxSpeedWithModifiers()
    {
        float modifier = Mathf.Max(speedModifierIntensity, isBoosting() ? boostIntensity : 0);

        return maxSpeed + modifier - GetContactSurfaceFriction() - (Mathf.Abs(steerValue) * steerVelocityFriction);
    }

    private void UpdateSpeedModifiers()
    {
        speedModifierReserveTime -= Time.fixedDeltaTime;
        if (speedModifierReserveTime < 0)
        {
            speedModifierReserveTime = 0;
            speedModifierIntensity = 0;
        }
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
            vehicleProxy.AddForce((targetSpeed - forwardSpeed) * vehicleBox.transform.forward, ForceMode.VelocityChange);
        }
    }

    private void RaycastForContactSurface()
    {
        Ray ray = new Ray(vehicleProxy.transform.position, -Vector3.up);

        // TODO: make track surface and track wall layer
        LayerMask layerMask = LayerMask.GetMask("Default");
        bHit = Physics.Raycast(ray, out hit, rayDist, layerMask);

        Debug.DrawLine(vehicleProxy.transform.position, vehicleProxy.transform.position - Vector3.up * rayDist);
    }

    private void Gravity()
    {
        // if in touch with ground or not attempting to move is world down vector otherwise is ground normal
        Vector3 gravityDir = vInput != 0 && bHit ? -hit.normal : -Vector3.up;

        // gravity force
        vehicleProxy.AddForce(gravityDir * gravityStr, ForceMode.Acceleration);
    }

    private void ApplySteer()
    {
        // applying vehicle yaw to the box
        steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime;
        steerValue = forwardSpeed > 0 ? steerValue : -steerValue;

        vehicleBox.transform.rotation = vehicleBox.transform.rotation * Quaternion.AngleAxis(steerValue, vehicleBox.transform.up);
    }

    private void AlignWithContactSurface()
    {
        // if in touch with ground align with surface normal otherwise align with world up 
        Vector3 boxUp = bHit ? hit.normal : Vector3.up;
        Vector3 nForward = Vector3.Normalize(Vector3.Cross(vehicleBox.transform.right, boxUp));
        Quaternion q = Quaternion.LookRotation(nForward, boxUp);

        vehicleBox.transform.rotation = q;
    }

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

        UpdateSpeedModifiers();

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


        RaycastForContactSurface();

        Gravity();

        ApplySteer();

        AlignWithContactSurface();

        maxSpeedWithModifier = GetMaxSpeedWithModifiers();

        UpdateAeroState();

        if (!bHit)
        {


        }
        if (bHit)
        {

            // if boosting set speed to max
            if (maxSpeed < maxSpeedWithModifier)
            {
                IncreaseSpeedTo(maxSpeedWithModifier);
            }

            float enginePower = Mathf.Abs(forwardSpeed) < maxSpeedWithModifier ? accel : 0;
            enginePower *= vInput;

            vehicleProxy.AddForce(vehicleBox.transform.forward * enginePower, ForceMode.Acceleration);

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : slipingSpeed / vehicleProxy.velocity.magnitude;


            if (Mathf.Abs(slipingSpeedRatio) > 0)
            {
                float t = drifting ? driftTraction : traction;

                vehicleProxy.AddForce(-slipingSpeed * t * vehicleBox.transform.right, ForceMode.VelocityChange);

                tractionText.text = string.Format("Traction = {0:F2}", t);
            }

        }


        vehicleProxy.AddForce((vehicleProxy.velocity.magnitude == 0 ? 0 : counterForceStr) * -vehicleProxy.velocity.normalized, ForceMode.VelocityChange);

        if (jumping)
        {
            OnStartJump();
        }

        speedText.text = string.Format("Speed : {0:F2}", forwardSpeed);
    }

    void Update()
    {

    }

    private void LateUpdate()
    {
        
    }
}
