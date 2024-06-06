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

    public AnimationCurve engineCurve;
    public float maxSpeed = 20;
    public float engineTorque = 2000;
    public AnimationCurve brakeCurve;
    public float maxBrake = 5;
    public float brakeTorque = 2000;
    [HideInInspector]
    public float currentTorque = 0.0f;

    public float rotationTorque = 1000;
    public AnimationCurve tractionCurve;

    [HideInInspector]
    int numWheelsOnGround = 0;

    [HideInInspector]
    float slipingRatio;

    [HideInInspector]
    float traction;

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

    void OnGUI()
    {
        Vector2 TextPosition = new Vector2(Screen.width - 200, 50);

        string speedText = "speed : " + Mathf.Floor(rb.velocity.magnitude);
        Vector2 size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(speedText));
        GUI.Label(new Rect(TextPosition, new Vector2(size.x, size.y)), speedText);

        string numberOfWheelsOnGroundText = "number of wheels on ground : " + numWheelsOnGround;
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(numberOfWheelsOnGroundText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y), new Vector2(size.x, size.y)), numberOfWheelsOnGroundText);

        string tractionText = "traction : " + Mathf.Floor(traction * 100) / 100;
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(tractionText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 2), new Vector2(size.x, size.y)), tractionText);

        string vinputText = "vInput : " + Mathf.Floor(vInput * 100) / 100;
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(vinputText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 3), new Vector2(size.x, size.y)), vinputText);

        string hInputText = "hInput : " + Mathf.Floor(hInput * 100) / 100;
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(hInputText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 4), new Vector2(size.x, size.y)), hInputText);

        string torqueText = "Torque : " + Mathf.Floor(currentTorque);
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(torqueText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 5), new Vector2(size.x, size.y)), torqueText);
    }

    void FixedUpdate()
    {
        // Gravity
        Vector3 GravityForce = gravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        rb.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);

        //DrawHelpers.DrawSphere(rb.worldCenterOfMass, .2f, Color.black);

        numWheelsOnGround = 0;
        foreach (var wheel in wheels)
        {
            if (wheel.isOnGround)
                numWheelsOnGround++;
        }

        if (numWheelsOnGround > 2) 
        {
            //rb.AddTorque(hInput * transform.up * rotationTorque);

            slipingRatio = rb.velocity.magnitude == 0 ? 0.0f : Vector3.Dot(transform.right, rb.velocity) / rb.velocity.magnitude;

            if (slipingRatio != 0)
            {
                float velocityRatio = Mathf.Clamp(rb.velocity.magnitude == 0 ? 0 : rb.velocity.magnitude / maxSpeed, 0, 1);
                //traction = tractionCurve.Evaluate(velocityRatio);
                traction = tractionCurve.Evaluate(slipingRatio);

                Vector3 slipingVelocity = rb.velocity.magnitude * -transform.right * slipingRatio * traction;

                //rb.AddForce(slipingVelocity, ForceMode.VelocityChange);
            }

            else
            {
                traction = 0.0f;
            }
        }

        bool Accerating = Vector3.Dot(rb.velocity, transform.forward) > 0;

        if(vInput > 0)
        {
            float speedRatio = rb.velocity.magnitude / maxSpeed;
            speedRatio = Accerating ? speedRatio : 0;

            if (speedRatio < 1)
            {
                currentTorque = engineCurve.Evaluate(speedRatio);
                currentTorque *= engineTorque;
            }
            else
            {
                currentTorque = 0.0f;
            }
        }
        else
        {
            float speedRatio = rb.velocity.magnitude / maxBrake;
            speedRatio = Accerating ? 0 : speedRatio;

            if (speedRatio < 1)
            {
                currentTorque = brakeCurve.Evaluate(speedRatio);
                currentTorque *= brakeTorque;
            }
            else
            {
                currentTorque = 0.0f;
            }

        }

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
    }



}
