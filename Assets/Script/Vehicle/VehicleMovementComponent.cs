using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows;

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

    public float vInput;
    public float hInput;


    VehicleWheel[] wheels = new VehicleWheel[4];

    public RoadSplineRimporter RoadSpline;

    void Start()
    {
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += Vector3.up * centreOfMassOffset;

        wheels = GetComponentsInChildren<VehicleWheel>();


//         foreach (VehicleWheel wheel in allWheels)
//         {
//             WheelFrictionCurve c = wheel.WheelCollider.forwardFriction;
//             c.asymptoteSlip = 0;
//             c.asymptoteValue = 0;
//             c.extremumSlip = 0;
//             c.extremumValue = 0;
//             c.stiffness = 0;
// 
//             WheelFrictionCurve d = wheel.WheelCollider.sidewaysFriction;
//             c.asymptoteSlip = 0;
//             c.asymptoteValue = 0;
//             c.extremumSlip = 0;
//             c.extremumValue = 0;
//             c.stiffness = 0;
//             if (wheel.name == "Wheel_FL")
//                 wheels[0] = wheel;
//             else if (wheel.name == "Wheel_FR")
//                 wheels[1] = wheel;
//             else if (wheel.name == "Wheel_RL")
//                 wheels[2] = wheel;
//             else if (wheel.name == "Wheel_RR")
//                 wheels[3] = wheel;
//         }

        gravityDirection = Vector3.down;

    }

    void FixedUpdate()
    {
        // Gravity
        Vector3 GravityForce = gravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        rb.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);

        DrawHelpers.DrawSphere(rb.worldCenterOfMass, .2f, Color.black);

//         int numWheelsOnGround = 0;
//         foreach (var wheel in wheels)
//         {
//             if (wheel.isOnGround)
//                 numWheelsOnGround++;
//         }
// 
//         if (numWheelsOnGround > 2) 
//         {
//             Vector3 force = vInput * transform.forward * maxSpeed;
//             rb.AddForce(force);
//         }
    }

    private void Update()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

        foreach (Touch t in UnityEngine.Input.touches)
        {
            Vector2 p = t.position;

            if (p.x < Screen.width / 2)
                vInput -= 1;
            else
                vInput += 1;
        }

        float angle = 0;
        if (UnityEngine.Input.acceleration != Vector3.zero)
        {
            angle = Mathf.Atan2(UnityEngine.Input.acceleration.x, -UnityEngine.Input.acceleration.y) * Mathf.Rad2Deg;
        }

        float axisValue = Mathf.InverseLerp(-40, 40, angle) * 2 - 1;
        hInput += axisValue;

        foreach (var wheel in wheels)
        {
            wheel.throtleInput = vInput;
        }
    }



}
