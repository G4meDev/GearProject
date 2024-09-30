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
            target.transform.position = vehicle.vehicleBox.transform.position;

            Vector3 cameraUp = vehicle.orientNode.GetCameraUpVector(vehicle.vehicleBox.transform.position);
            Vector3 forward = Vector3.Cross(vehicle.vehicleBox.transform.right, cameraUp);

            target.transform.rotation = Quaternion.LookRotation(forward, cameraUp);
        }
    }
}