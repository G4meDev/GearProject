using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
    public new Camera camera;
    public GameObject target;

    private Vehicle vehicle;

    void Start()
    {
        vehicle = transform.root.GetComponentInChildren<Vehicle>();
    }
    
    void FixedUpdate()
    {
        if (vehicle)
        {
            //target.transform.position = vehicle.vehicleBox.transform.position;

            Vector3 cameraUp = Vector3.up;
            Vector3 cameraForward = Vector3.forward;
            Vector3 cameraPos = Vector3.zero;

            if(vehicle.orientNode)
            {
                vehicle.orientNode.GetCameraVectors(vehicle.vehicleBox.transform.position, out cameraForward, out cameraUp, out cameraPos);
            }

            target.transform.position = cameraPos;

            target.transform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);
        }
    }
}