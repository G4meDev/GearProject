using Cinemachine;
using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
    public new UnityEngine.Camera camera;
    public GameObject target;

    public Vehicle vehicle;

    private CameraRail cameraRail;

    void Start()
    {
        cameraRail = GameObject.FindObjectOfType<CameraRail>();
    }

    void LateUpdate()
    {
        if (vehicle && cameraRail)
        {
            cameraRail.GetCameraVectors(vehicle.vehicleMesh.transform.position, out Vector3 cameraForward, out Vector3 cameraUp, out Vector3 cameraPos);
            //target.transform.position = cameraPos;
            target.transform.position = (cameraPos + vehicle.vehicleMesh.transform.position) / 2;
            
            //target.transform.position = vehicle.vehicleMesh.transform.position;


            target.transform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);
            //target.transform.rotation = vehicle.vehicleMesh.transform.rotation;
        }
    }
}