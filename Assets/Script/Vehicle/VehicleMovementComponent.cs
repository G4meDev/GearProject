using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class VehicleMovementComponent : MonoBehaviour
{
    private Rigidbody RB;
    private Vector3 GravityDirection;

    void Start()
    {
        RB = GetComponent<Rigidbody>();

        GravityDirection = Vector3.down;
    }

    

    void FixedUpdate()
    {
        Vector3 GravityForce = GravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        RB.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);


    }
}
