using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Orient_Node : MonoBehaviour
{
    public List<Camera_Orient_Node> Neighbours;

    private void OnTriggerEnter(Collider other)
    {
        SC_TestVehicle vehicle = other.transform.root.GetComponentInChildren<SC_TestVehicle>();
        vehicle.orientNode = this;

        Debug.Log("camera");
    }

    public Vector3 GetCameraUpVector(Vector3 worldPos)
    {
        float min_dist = float.MaxValue;
        Camera_Orient_Node nearest = null;

        foreach (Camera_Orient_Node node in Neighbours)
        {
            float dist = Vector3.Distance(node.transform.position, worldPos);
            if (dist < min_dist)
            {
                min_dist = dist;
                nearest = node;
            }
        }

        Vector3 d = nearest.transform.position - transform.position;

        Vector3 vec1 = d.normalized;
        Vector3 vec2 = worldPos - transform.position;

        float a = Mathf.Clamp01(Vector3.Dot(vec1, vec2) / d.magnitude);

        return Vector3.Lerp(transform.up, nearest.transform.up, a);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
