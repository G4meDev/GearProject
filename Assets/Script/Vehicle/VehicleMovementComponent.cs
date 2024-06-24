using System.ComponentModel;
using UnityEngine;

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

    public float maxDownForce = 10;

    public AnimationCurve steerCurve;
    [HideInInspector]
    public float steerValue;

    [HideInInspector]
    int numWheelsOnGround = 0;

    [HideInInspector]
    public float speedRatio;
    [HideInInspector]
    public float forwardSpeed;
    [HideInInspector]
    public float rightSpeed;
    [HideInInspector]
    public float forwardSkid;
    [HideInInspector]
    public float rightSkid;

    [HideInInspector]
    public bool accelerating = false;


    VehicleWheel[] wheels = new VehicleWheel[4];

    public GameObject roadSplineObject;

    [HideInInspector]
    public HEU_RoadSplineImporter roadSpline;

    [Space]
    public GearButton throttleButton;

    public GearButton brakeButton;

    public GearButton rightButton;

    public GearButton leftButton;


    void Start()
    {
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += Vector3.up * centreOfMassOffset;

        wheels = GetComponentsInChildren<VehicleWheel>();

        if(roadSplineObject)
            roadSpline = roadSplineObject.GetComponentInChildren<HEU_RoadSplineImporter>();

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

        string vinputText = "vInput : " + Mathf.Floor(vInput * 100) / 100;
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(vinputText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 2), new Vector2(size.x, size.y)), vinputText);

        string hInputText = "hInput : " + Mathf.Floor(hInput * 100) / 100;
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(hInputText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 3), new Vector2(size.x, size.y)), hInputText);

        string torqueText = "Torque : " + Mathf.Floor(currentTorque);
        size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(torqueText));
        GUI.Label(new Rect(TextPosition + new Vector2(0, size.y * 4), new Vector2(size.x, size.y)), torqueText);
    }

    void FixedUpdate()
    {
        UpdateSpeedParams();
        Gravity();
        UpdateNumWheelsOnGround();
        DownForce();
        EngineTorque();

        //DrawHelpers.DrawSphere(rb.worldCenterOfMass, .2f, Color.black);

    }

    private void Update()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

//         vInput += throttleButton.value - brakeButton.value;
//         hInput += rightButton.value - leftButton.value;


//         foreach (Touch t in UnityEngine.Input.touches)
//         {
//             Vector2 p = t.position;
// 
//             if (p.x < Screen.width / 2)
//                 vInput -= 1;
//             else
//                 vInput += 1;
//         }
// 
//         float angle = 0;
//         if (UnityEngine.Input.acceleration != Vector3.zero)
//         {
//             angle = Mathf.Atan2(UnityEngine.Input.acceleration.x, -UnityEngine.Input.acceleration.y) * Mathf.Rad2Deg;
//         }
// 
//         float axisValue = Mathf.InverseLerp(-40, 40, angle) * 2 - 1;
//         hInput += axisValue;

        float forwardSpeedRatio = Vector3.Dot(rb.velocity, transform.forward);
        float speedRatio = Mathf.Clamp01(forwardSpeedRatio == 0 ? 0 : forwardSpeedRatio / maxSpeed);
        steerValue = steerCurve.Evaluate(speedRatio);
    }

    private void Gravity()
    {
        if (roadSpline)
            gravityDirection = -roadSpline.GetClosestRoadSplinePoint(transform.position).up;
        else
            gravityDirection = Vector3.down;

        Debug.DrawLine(transform.position, transform.position + gravityDirection * -2);

        Vector3 GravityForce = gravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        rb.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);
    }

    private void UpdateSpeedParams()
    {
        speedRatio = rb.velocity.magnitude == 0 ? 0 : Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);

        forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);
        rightSpeed = Vector3.Dot(transform.right, rb.velocity);

        forwardSkid = rb.velocity.magnitude == 0 ? 0 : Mathf.Clamp01(forwardSpeed / rb.velocity.magnitude);
        rightSkid = rb.velocity.magnitude == 0 ? 0 : Mathf.Clamp01(rightSpeed / rb.velocity.magnitude);

        accelerating = forwardSpeed > 0;
    }

    private void UpdateNumWheelsOnGround()
    {
         numWheelsOnGround = 0;
         foreach (var wheel in wheels)
         {
             if (wheel.isOnGround)
                 numWheelsOnGround++;
         }
    }

    private void DownForce()
    {
        if(numWheelsOnGround == 4)
        {
            float downForce = speedRatio * maxDownForce;
            rb.AddForce(downForce * gravityDirection, ForceMode.Acceleration);
        }
    }

    private void EngineTorque()
    {
        if (vInput > 0)
        {
            currentTorque = engineCurve.Evaluate(accelerating ? speedRatio : 0);
            currentTorque *= engineTorque;
        }
        else if (vInput < 0)
        {         
            currentTorque = brakeCurve.Evaluate(accelerating ? 0 : speedRatio);
            currentTorque *= brakeTorque;
        }
    }
}

