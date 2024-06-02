using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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
        // Gravity
        Vector3 GravityForce = GravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        RB.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);


    }



}
