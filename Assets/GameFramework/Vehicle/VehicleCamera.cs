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
        //target.transform.position = vehicle.vehicleProxy.transform.position;

        if (vehicle && cameraRail)
        {
            cameraRail.GetCameraVectors(vehicle.vehicleProxy.transform.position, out Vector3 cameraForward, out Vector3 cameraUp, out Vector3 cameraPos);
            target.transform.position = cameraPos;
            //target.transform.position = vehicle.vehicleMesh.transform.position;
            target.transform.position = (cameraPos + vehicle.vehicleMesh.transform.position) / 2;


            target.transform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);
            //target.transform.rotation = Quaternion.LookRotation(cameraForward, Vector3.up);
            //target.transform.rotation = vehicle.vehicleMesh.transform.rotation;
        }
    }
}