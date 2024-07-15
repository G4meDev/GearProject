using UnityEngine;

public class VehicleWheel : MonoBehaviour
{
    [HideInInspector]
    public Transform WheelBoneTransform;

    [HideInInspector]
    public Transform axelTransform;

    [HideInInspector]
    public RaycastHit contactHit = new RaycastHit();

    [HideInInspector]
    public float offset = 0.0f;

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
    private float currentYaw = 0;

    [HideInInspector]
    private float currentRoll = 0;

    [HideInInspector]
    VehicleMovementComponent MovmentComp;
    
    [HideInInspector]
    Rigidbody CarBody;

    [HideInInspector]
    public bool onGround;
    //[HideInInspector]
    public Transform wheelTransform;


    void Start()
    {
        CarBody = transform.root.GetComponent<Rigidbody>();
        MovmentComp = CarBody.GetComponent<VehicleMovementComponent>();
    }

    private void Update()
    {
        currentYaw = effectedBySteer ? MovmentComp.hInput * MovmentComp.steerValue * maxSteerAngle : 0;

        
    }


    void FixedUpdate()
    {
        wheelTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        wheelTransform.Rotate(0, currentYaw, 0);

        Ray ray = new Ray(transform.position, -transform.up);
        onGround = Physics.Raycast(ray, out contactHit, SuspensionLength);

        if (onGround)
        {
            Vector3 SpringDir = transform.up;
            Vector3 tireWorldVelocity = CarBody.GetPointVelocity(wheelTransform.position);

            offset = SuspensionRestLength - contactHit.distance;
            float vel = Vector3.Dot(SpringDir, tireWorldVelocity);

            float suspenssionForce = (offset * springStrength) - (vel * springDamper);
            CarBody.AddForceAtPosition(SpringDir * suspenssionForce, wheelTransform.position, ForceMode.Acceleration);


            Vector3 t = wheelTransform.parent.InverseTransformPoint(CarBody.worldCenterOfMass);
            Vector3 targetLocal = new Vector3(wheelTransform.localPosition.x, t.y + forceComOffset, wheelTransform.localPosition.z);
            Vector3 targetWorld = wheelTransform.parent.TransformPoint(targetLocal);

            // ------------------------------------------------------
            
            if (effectedByEngine)
            {
                Vector3 contactNormal = contactHit.normal;
                Vector3 contactRightVector = Vector3.Cross(contactNormal, transform.forward);
                Vector3 contactTangent = Vector3.Cross(contactRightVector, contactNormal);

                Vector3 p = contactHit.point + new Vector3(0, 2, 0);
                Debug.DrawLine(p, p + contactTangent);

                Vector3 throtleForce = MovmentComp.vInput * MovmentComp.currentTorque * contactTangent;
                CarBody.AddForceAtPosition(throtleForce, targetWorld, ForceMode.Acceleration);
            }

            // ------------------------------------------------------

            Vector3 steerDir = wheelTransform.right;
            float steerVelocity = Vector3.Dot(steerDir, tireWorldVelocity);
            float steerRatio = steerVelocity == 0 ? 0 : steerVelocity / CarBody.velocity.magnitude;
            steerRatio = Mathf.Clamp(Mathf.Abs(steerRatio), 0, 1);
            float traction = tractionCurve.Evaluate(steerRatio);

            float desirdVelocityChange = -steerVelocity * traction;
            //float desireAccel = desirdVelocityChange / Time.fixedDeltaTime;


            //CarBody.AddForceAtPosition(steerDir * desireAccel * 0.25f, targetWorld, ForceMode.Acceleration);
            CarBody.AddForceAtPosition(steerDir * desirdVelocityChange * 0.25f, targetWorld, ForceMode.VelocityChange);
        }


        Vector3 c = onGround ? contactHit.point : transform.position - transform.parent.up * SuspensionLength;
        Vector3 boneTarget = c + transform.parent.up * wheelRadius;

        wheelTransform.position = boneTarget;


        Vector3 v = CarBody.GetPointVelocity(transform.position);
        float wheelForwardVelocity = Vector3.Dot(v, CarBody.transform.forward);
        float rotationAngle = (wheelForwardVelocity * 360 * Time.fixedDeltaTime) / Mathf.PI * 2 * wheelRadius;

        currentRoll += rotationAngle;
        wheelTransform.Rotate(currentRoll, 0, 0);
    }
}
