using System.Collections;
using System.Collections.Generic;
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

    Rigidbody CarBody;

    void Start()
    {
        Transform T = transform.root.Find("VehicleMesh/root/" + name);
        if (T)
            WheelBoneTransform = T;
        else
            Debug.LogError(name + ": no bone with name _ " + name);

        CarBody = transform.root.GetComponent<Rigidbody>();


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

            float force = (offset * springStrength) - (vel * springDamper);
            CarBody.AddForceAtPosition(SpringDir * force, transform.position);
        }

        Vector3 c = isOnGround ? contactHit.point : transform.position - transform.forward * SuspensionLength;
        WheelBoneTransform.position = c + transform.forward * wheelRadius;
    }
}
