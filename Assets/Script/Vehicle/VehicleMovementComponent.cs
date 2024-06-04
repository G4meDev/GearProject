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

    public float centreOfMassOffset = -1f;

    public float vInput;
    public float hInput;

    public AnimationCurve EngineCurve;
    public float maxSpeed = 20;
    public float engineTorque = 2000;
    [HideInInspector]
    public float currentTorque = 0.0f;

    public float rotationTorque = 1000;
    public float traction = 1;





    VehicleWheel[] wheels = new VehicleWheel[4];

    public RoadSplineRimporter RoadSpline;

    void Start()
    {
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += Vector3.up * centreOfMassOffset;

        wheels = GetComponentsInChildren<VehicleWheel>();

        if(RoadSpline)
            gravityDirection = -RoadSpline.GetClosestRoadSplinePoint(transform.position).up;
        else
            gravityDirection = Vector3.down;

    }

    void FixedUpdate()
    {
        // Gravity
        Vector3 GravityForce = gravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        rb.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);

        DrawHelpers.DrawSphere(rb.worldCenterOfMass, .2f, Color.black);

        int numWheelsOnGround = 0;
        foreach (var wheel in wheels)
        {
            if (wheel.isOnGround)
                numWheelsOnGround++;
        }

        if (numWheelsOnGround > 2) 
        {
            rb.AddTorque(hInput * transform.up * rotationTorque);

            float slipingRatio = rb.velocity.magnitude == 0 ? 0.0f : Vector3.Dot(transform.right, rb.velocity) / rb.velocity.magnitude;
            Debug.DrawLine(transform.position, transform.position + transform.right * 2);
            //Debug.Log("slip : " + Mathf.Floor(slipingRatio * 100) / 100);

            if (slipingRatio != 0)
            {
                Vector3 slipingVelocity = rb.velocity.magnitude * -transform.right * slipingRatio * traction;

                rb.AddForce(slipingVelocity, ForceMode.VelocityChange);
            }
        }

        float speedRatio = rb.velocity.magnitude / maxSpeed;
        if (speedRatio < 1)
        {
            currentTorque = EngineCurve.Evaluate(speedRatio);
            currentTorque *= engineTorque;
        }
        else
        {
            currentTorque = 0.0f;
        }

        Debug.Log(rb.velocity.magnitude);
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
