using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class VehicleMovementComponent : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 gravityDirection;

    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfMassOffset = -1f;
    public float AntiRoll= 5000.0f;

    VehicleWheel[] wheels = new VehicleWheel[4];

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += Vector3.up * centreOfMassOffset;

        VehicleWheel[] allWheels = GetComponentsInChildren<VehicleWheel>();

        foreach (VehicleWheel wheel in allWheels) 
        {
            if (wheel.name == "Wheel_FL")
                wheels[0] = wheel;
            else if (wheel.name == "Wheel_FR")
                wheels[1] = wheel;
            else if (wheel.name == "Wheel_RL")
                wheels[2] = wheel;
            else if (wheel.name == "Wheel_RR")
                wheels[3] = wheel;
        }

        gravityDirection = Vector3.down;

    }

    void FixedUpdate()
    {
        // Gravity
        Vector3 GravityForce = gravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        rb.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);

        AntiRollForAxis(wheels[0].WheelCollider, wheels[1].WheelCollider);
        AntiRollForAxis(wheels[2].WheelCollider, wheels[3].WheelCollider);
    }

    private void Update()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        float forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        DrawHelpers.DrawSphere(rb.worldCenterOfMass, 1, Color.green);





        foreach (var wheel in wheels)
        {
            if (wheel.CanSteer)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isAccelerating)
            {
                if (wheel.EffectedByEngine)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }


    void AntiRollForAxis(WheelCollider L, WheelCollider R)
    {
        WheelHit hit;

        float travelL = 1.0f;
        float travelR = 1.0f;

        var groundedL = L.GetGroundHit(out hit);
        if (groundedL)
        { 
            travelL = (-L.transform.InverseTransformPoint(hit.point).y - L.radius) / L.suspensionDistance;
        }

        var groundedR = R.GetGroundHit(out hit);
        if (groundedR)
        {
            travelL = (-R.transform.InverseTransformPoint(hit.point).y - R.radius) / R.suspensionDistance;
        }

        var antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
            rb.AddForceAtPosition(L.transform.up * -antiRollForce, L.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(R.transform.up * antiRollForce, R.transform.position);

    }
}
