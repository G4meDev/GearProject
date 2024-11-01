using Cinemachine;
using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
    public new UnityEngine.Camera camera;
    public GameObject target;

    private Vehicle vehicle;

    private CameraRail cameraRail;

    void Start()
    {
        vehicle = transform.root.GetComponentInChildren<Vehicle>();
        cameraRail = GameObject.FindObjectOfType<CameraRail>();
    }
    
    void LateUpdate()
    {
        if (cameraRail)
        {
            cameraRail.GetCameraVectors(vehicle.vehicleMesh.transform.position, out Vector3 cameraForward, out Vector3 cameraUp, out Vector3 cameraPos);

            //target.transform.position = cameraPos;
            //target.transform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);

            target.transform.position = vehicle.vehicleMesh.transform.position;

            DrawHelpers.DrawSphere(cameraPos, 2, Color.black);
            DrawArrow.ForDebug(cameraPos + Vector3.up * 2, cameraForward.normalized);
        }

    }
}