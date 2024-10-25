using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class CameraRail : MonoBehaviour
{
    private Spline spline;

    void Start()
    {
        spline = GetComponent<SplineContainer>().Spline;
    }

    public void GetCameraVectors(Vector3 worldPos, out Vector3 forwardVector, out Vector3 upVector, out Vector3 cameraPos)
    {
        SplineUtility.GetNearestPoint<Spline>(spline, worldPos, out Unity.Mathematics.float3 nearestPos, out float t);
        SplineUtility.Evaluate<Spline>(spline, t, out Unity.Mathematics.float3 pos, out Unity.Mathematics.float3 forward, out Unity.Mathematics.float3 up);
        
        cameraPos = new(pos.x, pos.y, pos.z);
        forwardVector = new(forward.x, forward.y, forward.z);
        upVector = new(up.x, up.y, up.z);
    }

        void Update()
    {
        
    }
}
