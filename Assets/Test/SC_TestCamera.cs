using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TestCamera : MonoBehaviour
{
    public GameObject vehicleMesh;

    public float lagSpeed = 15;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private Vector3 velo = Vector3.zero; 

    private void updateTransform()
    {
        Vector3 newForward = Vector3.Normalize(Vector3.Cross(vehicleMesh.transform.right, Vector3.up));

        targetPos = vehicleMesh.transform.TransformPoint(new Vector3(0, 2, -5));
        targetRot = Quaternion.LookRotation(newForward);
    }

    void Start()
    {
        updateTransform();

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
    
    void Update()
    {
        Vector3 newForward = Vector3.Normalize(Vector3.Cross(vehicleMesh.transform.right, Vector3.up));

        targetPos = vehicleMesh.transform.TransformPoint(new Vector3(0, 2, -5));
        targetRot = Quaternion.LookRotation(newForward);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velo, lagSpeed);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1);
        transform.rotation = targetRot;
    }
}
