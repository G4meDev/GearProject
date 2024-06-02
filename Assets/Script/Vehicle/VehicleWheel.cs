using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleWheel : MonoBehaviour
{
    public Transform WheelBoneTransform;

    [HideInInspector] public WheelCollider WheelCollider;

    public bool CanSteer;
    public bool EffectedByEngine;

    Vector3 position;
    Quaternion rotation;

    void Start()
    {
        WheelCollider = GetComponent<WheelCollider>();

        Transform T = transform.root.Find("VehicleMesh/root/" + name);
        if (T)
            WheelBoneTransform = T;
        else
            Debug.LogError(name + ": no bone with name _ " + name);
        


    }

    void Update()
    {
        if (WheelBoneTransform) 
        {
            WheelCollider.GetWorldPose(out position, out rotation);

            rotation = rotation * Quaternion.AngleAxis(90, Vector3.forward);

            WheelBoneTransform.position = position;
            WheelBoneTransform.rotation = rotation;
        }
    }
}
