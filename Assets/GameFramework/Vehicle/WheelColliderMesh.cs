using HoudiniEngineUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelColliderMesh : MonoBehaviour
{
    public WheelCollider wheel;

    void Update()
    {
        if (wheel)
        {
            wheel.GetWorldPose(out Vector3 worldPos, out Quaternion worldRot);

            Vector3 localPos = transform.localPosition;
            localPos.y = wheel.contactOffset;

            transform.localPosition = localPos;

            transform.rotation = worldRot;
            transform.Rotate(new Vector3(0, -90, 0), Space.Self);
        }
    }
}
