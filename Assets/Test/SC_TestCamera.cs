using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_TestCamera : MonoBehaviour
{
    public GameObject vehicleMesh;

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 newForward = Vector3.Normalize(Vector3.Cross(vehicleMesh.transform.right, Vector3.up));

        transform.position = vehicleMesh.transform.position + (newForward * -5) + (Vector3.up * 2);
        transform.LookAt(vehicleMesh.transform);
    }
}
