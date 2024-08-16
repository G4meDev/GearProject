using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TestVehicleMesh : MonoBehaviour
{
    public GameObject VehicleBox;
    public GameObject cameraTarget;

    public Vector3 meshOffset;

    public float lerpRate = 10.0f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        transform.position = VehicleBox.transform.TransformPoint(meshOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, VehicleBox.transform.rotation, Time.fixedDeltaTime * lerpRate);

        Vector3 newForward = Vector3.Normalize(Vector3.Cross(transform.right, Vector3.up));
        cameraTarget.transform.position = transform.position + newForward * 10;
    }

}
