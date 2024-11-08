using HoudiniEngineUnity;
using UnityEngine;
using UnityEngine.Windows;

public class VehicleWheel : MonoBehaviour
{
    [HideInInspector]
    public Vehicle vehicle;

    [HideInInspector]
    public RaycastHit contactHit = new RaycastHit();

    [HideInInspector]
    public float offset = 0.0f;

    public float SuspensionRestLength = 0.25f;

    public float springStrength = 1000;
    public float springDamper = 150;

    public float wheelRadius = 0.22f;

    public AnimationCurve tractionCurve;

    public bool effectedByEngine = true;
    public bool effectedBySteer = true;

    public float forceComOffset;

    [HideInInspector]
    public bool onGround;

    [HideInInspector]
    public Vector3 worldVelocity;

    private void Awake()
    {
        vehicle = transform.root.GetComponent<Vehicle>();
    }

    public void StepPhysic()
    {
        if (vehicle)
        {
            if (effectedBySteer)
            {
                transform.localRotation = Quaternion.Euler(0, vehicle.currentSteer, 0);
            }

            Vector3 SpringDir = transform.up;
            Debug.DrawLine(transform.position, transform.position + -SpringDir * SuspensionRestLength);

            Ray ray = new Ray(transform.position, -SpringDir);
            onGround = Physics.Raycast(ray, out contactHit, SuspensionRestLength);

            if (onGround)
            {
                Vector3 t = transform.parent.InverseTransformPointUnscaled(vehicle.vehicleProxy.worldCenterOfMass);
                Vector3 targetLocal = new Vector3(transform.localPosition.x, t.y + forceComOffset, transform.localPosition.z);
                Vector3 targetWorld = transform.parent.TransformPointUnscaled(targetLocal);

                DrawHelpers.DrawSphere(contactHit.point, .1f, Color.blue);

                worldVelocity = vehicle.vehicleProxy.GetPointVelocity(targetWorld);
                offset = SuspensionRestLength - contactHit.distance;
                float vel = Vector3.Dot(SpringDir, worldVelocity);

                float suspenssionForce = (offset * springStrength) - (vel * springDamper);

                vehicle.vehicleProxy.AddForceAtPosition(SpringDir * suspenssionForce, targetWorld, ForceMode.Acceleration);

                if (effectedByEngine)
                {
                    Vector3 throtleForce = vehicle.avaliableTorque * transform.forward;
                    vehicle.vehicleProxy.AddForceAtPosition(throtleForce, targetWorld, ForceMode.Acceleration);

                    Debug.DrawLine(targetWorld, targetWorld + throtleForce * 1);
                }

                Vector3 steerDir = transform.right;
                float steerVelocity = Vector3.Dot(steerDir, worldVelocity);
                float steerRatio = steerVelocity == 0 ? 0 : steerVelocity / worldVelocity.magnitude;
                steerRatio = Mathf.Clamp(Mathf.Abs(steerRatio), 0, 1);
                float traction = tractionCurve.Evaluate(steerRatio);

                //Debug.Log(steerRatio + "    " + traction);

                float desirdVelocityChange = -steerVelocity * traction;
                vehicle.vehicleProxy.AddForceAtPosition(steerDir * desirdVelocityChange * 0.25f, targetWorld, ForceMode.VelocityChange);
            }
        }
    }

    void Start()
    {

    }

    private void Update()
    {

    }


    void FixedUpdate()
    {

    }
}
