using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

[ExecuteInEditMode]
public class VehicleCameraArm : MonoBehaviour
{
    public float armLength = 1.0f;

    public Vector3 socketOffset = Vector3.zero;

    public float lagSpeed = 10;

    public float minFOV = 60;

    public float maxFOV = 90;


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
            camComp.fieldOfView = Mathf.Lerp(minFOV, maxFOV, MovementComp.speedR);

            updateTransforms();

            socket.position = Vector3.Lerp(lastPositionWs, socket.position, Mathf.Clamp01(lagSpeed * Time.deltaTime));


            lastPositionWs = socket.position;

            socket.rotation = Quaternion.LookRotation(target.position - socket.position, transform.parent.up);
        }

        else
        {
            updateTransforms();
        }

        Debug.DrawLine(socket.position, target.position);
    }
}