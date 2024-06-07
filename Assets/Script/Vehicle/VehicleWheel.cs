using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VehicleWheel : MonoBehaviour
{
    public Transform WheelBoneTransform;

    public bool CanSteer;
    public bool EffectedByEngine;

    [HideInInspector]
    public bool isOnGround = false;

    [HideInInspector]
    public RaycastHit contactHit = new RaycastHit();

    [HideInInspector]
    public float offset = 0.0f;

    public float mass = 20.0f;

    public float SuspensionLength = 0.5f;
    public float SuspensionRestLength = 0.25f;

    public float springStrength = 1000;
    public float springDamper = 150;

    public float wheelRadius = 0.22f;

    public AnimationCurve tractionCurve;

    public bool effectedByEngine = true;
    public bool effectedBySteer = true;
    public float maxSteerAngle = 40.0f;

    public float forceComOffset;

    [HideInInspector]
    public float currentYaw = 0;

    [HideInInspector]
    public float currentRoll = 0;

    [HideInInspector]
    VehicleMovementComponent MovmentComp;
    
    [HideInInspector]
    Rigidbody CarBody;

    [HideInInspector]
    GameObject refWheelTransform;

    void Start()
    {
        Transform T = transform.root.Find("VehicleMesh/root/" + name);
        if (T)
            WheelBoneTransform = T;
        else
            Debug.LogError(name + ": no bone with name _ " + name);

        CarBody = transform.root.GetComponent<Rigidbody>();
        MovmentComp = CarBody.GetComponent<VehicleMovementComponent>();

        refWheelTransform = new GameObject(name + "_Refrence");
        refWheelTransform.transform.parent = transform.parent;
        refWheelTransform.transform.transform.position = transform.position;
        refWheelTransform.transform.transform.rotation = transform.rotation;
    }

    private void Update()
    {
        currentYaw = effectedBySteer ? MovmentComp.hInput * MovmentComp.steerValue * maxSteerAngle : 0;

        Vector3 tireWorldVelocity = CarBody.GetPointVelocity(refWheelTransform.transform.position);
        float wheelForardVelocity = Vector3.Dot(tireWorldVelocity, refWheelTransform.transform.forward);
        float rotationAngle = (wheelForardVelocity * 360 * Time.deltaTime) / Mathf.PI * 2 * wheelRadius;

        currentRoll += rotationAngle;
    }


    void FixedUpdate()
    {
        transform.rotation = refWheelTransform.transform.rotation;
        transform.Rotate(0, currentYaw, 0);

        Debug.DrawLine(transform.position, transform.position + transform.forward * 0.5f, Color.blue);
        Debug.DrawLine(transform.position, transform.position + transform.right * 0.5f, Color.red);



        Ray ray = new Ray(refWheelTransform.transform.position, -refWheelTransform.transform.up);
        isOnGround = Physics.Raycast(ray, out contactHit, SuspensionLength);

        if (isOnGround)
        {
            Vector3 SpringDir = transform.up;
            Vector3 tireWorldVelocity = transform.root.GetComponent<Rigidbody>().GetPointVelocity(transform.position);

            offset = SuspensionRestLength - contactHit.distance;
            float vel = Vector3.Dot(SpringDir, tireWorldVelocity);

            float suspenssionForce = (offset * springStrength) - (vel * springDamper);
            CarBody.AddForceAtPosition(SpringDir * suspenssionForce, transform.position);


            Vector3 t = transform.parent.InverseTransformPoint(CarBody.worldCenterOfMass);
            Vector3 targetLocal = new Vector3(transform.localPosition.x, t.y + forceComOffset, transform.localPosition.z);
            Vector3 targetWorld = transform.parent.TransformPoint(targetLocal);
            DrawHelpers.DrawSphere(targetWorld, 0.2f, Color.blue);

            // ------------------------------------------------------
            if (effectedByEngine)
            {
                Vector3 contactNormal = contactHit.normal;
                Vector3 contactRightVector = Vector3.Cross(contactNormal, transform.forward);
                Vector3 contactTangent = Vector3.Cross(contactRightVector, contactNormal);

                Vector3 p = contactHit.point + new Vector3(0, 2, 0);
                Debug.DrawLine(p, p + contactTangent);

                Vector3 throtleForce = MovmentComp.vInput * MovmentComp.currentTorque * contactTangent;
                CarBody.AddForceAtPosition(throtleForce, targetWorld);
            }

            Vector3 steerDir = transform.right;
            float steerVelocity = Vector3.Dot(steerDir, tireWorldVelocity);
            float steerRatio = steerVelocity == 0 ? 0 : steerVelocity / CarBody.velocity.magnitude;
            steerRatio = Mathf.Clamp(Mathf.Abs(steerRatio), 0, 1);
            float traction = tractionCurve.Evaluate(steerRatio);

            float desirdVelocityChange = -steerVelocity * traction;
            float desireAccel = desirdVelocityChange / Time.fixedDeltaTime;

            
            CarBody.AddForceAtPosition(steerDir * desireAccel * mass, targetWorld);
          
        }


        Vector3 c = isOnGround ? contactHit.point : refWheelTransform.transform.position - refWheelTransform.transform.parent.up * SuspensionLength;
        WheelBoneTransform.position = c + refWheelTransform.transform.parent.up * wheelRadius;

        WheelBoneTransform.Rotate(currentRoll, 0, 0);
        Debug.Log(currentRoll);
    }
}
