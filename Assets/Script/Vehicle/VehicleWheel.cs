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

    public float SuspensionLength = 0.5f;
    public float SuspensionRestLength = 0.25f;

    public float springStrength = 1000;
    public float springDamper = 150;

    public float wheelRadius = 0.22f;

    public bool effectedByEngine = true;
    public bool effectedBySteer = true;

    public Vector3 forceTargetoffset;

    VehicleMovementComponent MovmentComp;
    Rigidbody CarBody;

    void Start()
    {
        Transform T = transform.root.Find("VehicleMesh/root/" + name);
        if (T)
            WheelBoneTransform = T;
        else
            Debug.LogError(name + ": no bone with name _ " + name);

        CarBody = transform.root.GetComponent<Rigidbody>();
        MovmentComp = CarBody.GetComponent<VehicleMovementComponent>();


    }

    private void Update()
    {
        Vector3 tireWorldVelocity = transform.root.GetComponent<Rigidbody>().GetPointVelocity(transform.position);
        float wheelForardVelocity = Vector3.Dot(tireWorldVelocity, transform.right);
        float rotationAngle = (wheelForardVelocity * 360 * Time.deltaTime) / Mathf.PI * 2 * wheelRadius;
        WheelBoneTransform.Rotate(0, rotationAngle, 0);
    }


    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, -transform.forward);
        isOnGround = Physics.Raycast(ray, out contactHit, SuspensionLength);

        Debug.DrawLine(transform.position, transform.position - transform.forward * SuspensionLength);

        if (isOnGround)
        {
            Vector3 SpringDir = transform.forward;
            Vector3 tireWorldVelocity = transform.root.GetComponent<Rigidbody>().GetPointVelocity(transform.position);

            offset = SuspensionRestLength - contactHit.distance;
            float vel = Vector3.Dot(SpringDir, tireWorldVelocity);

            float suspenssionForce = (offset * springStrength) - (vel * springDamper);
            CarBody.AddForceAtPosition(SpringDir * suspenssionForce, transform.position);

            // ------------------------------------------------------
            if (effectedByEngine)
            {
                Vector3 contactNormal = contactHit.normal;
                Vector3 contactRightVector = Vector3.Cross(contactNormal, -transform.right);
                Vector3 contactTangent = Vector3.Cross(contactRightVector, contactNormal);

                Vector3 throtleForce = MovmentComp.vInput * MovmentComp.currentTorque * contactTangent;
                Vector3 forceTarget = transform.TransformPoint(forceTargetoffset);
                CarBody.AddForceAtPosition(throtleForce, forceTarget);

                Debug.DrawLine(transform.position, transform.position + contactTangent * 1);
                DrawHelpers.DrawSphere(forceTarget, 0.2f, Color.blue);
            }
        }

        Vector3 c = isOnGround ? contactHit.point : transform.position - transform.forward * SuspensionLength;
        WheelBoneTransform.position = c + transform.forward * wheelRadius;
    }
}
