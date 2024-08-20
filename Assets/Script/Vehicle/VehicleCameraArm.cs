using UnityEngine;

[ExecuteInEditMode]
public class VehicleCameraArm : MonoBehaviour
{
    public float armLength = 1.0f;

    public Vector3 socketOffset = Vector3.zero;

    public float lagSpeed = 10;

    public float FOV = 60;

    public float FOVChangeStr = 0.001f;

    public float cameraShakeStr = 0.6f;


    [HideInInspector]
    public VehicleMovementComponent MovementComp;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public Transform socket;
    [HideInInspector]
    public Transform cam;
    [HideInInspector]
    public Camera camComp;

    [HideInInspector]
    public Vector3 lastPositionWs;



    Vector3 socketVelocity;

    public void updateTransforms()
    {
        target.transform.localPosition = Vector3.zero;
        socket.transform.localPosition = socketOffset + Vector3.forward * -armLength;
        socket.rotation = Quaternion.identity;
    }

    void Start()
    {
        target  = transform.GetChild(0);
        socket  = target.GetChild(0);
        cam     = socket.GetChild(0);

        camComp = cam.GetComponent<Camera>();

        MovementComp = transform.root.GetComponentInChildren<VehicleMovementComponent>();

        updateTransforms();
        socket.rotation = Quaternion.LookRotation(target.position - socket.position, transform.parent.up);

        lastPositionWs = socket.transform.position;
    }

    void LateUpdate()
    {
        if (Application.isPlaying) 
        {
            camComp.fieldOfView = FOV + (MovementComp.forwardSpeed * MovementComp.forwardSpeed * FOVChangeStr);

            updateTransforms();
            
            socket.position = Vector3.Lerp(lastPositionWs, socket.position, Mathf.Clamp01(lagSpeed * Time.deltaTime));

            lastPositionWs = socket.position;

            socket.rotation = Quaternion.LookRotation(target.position - socket.position, -MovementComp.gravityDirection);

            float t = Time.time;
            float x = Mathf.PerlinNoise(t, t);
            float y = Mathf.PerlinNoise((t+3) + 13, t);
            float z = Mathf.PerlinNoise((t + 7) + 5, t);

            Vector3 d = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f);

            cam.transform.localPosition = d * cameraShakeStr * MovementComp.forwardSpeed * MovementComp.forwardSpeed;
        }

        else
        {
            updateTransforms();
        }

        Debug.DrawLine(socket.position, target.position);
    }
}