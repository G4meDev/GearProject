using UnityEngine;

public class WheelMesh : MonoBehaviour
{
    public VehicleWheel wheel;

    public float wheelRadius = 0.3f;

    public float currentRoll = 0.0f;

    [HideInInspector]
    public Vector3 refrencePosition;

    [HideInInspector]
    public Quaternion refrenceRotation;

    private void Awake()
    {
        refrencePosition = transform.localPosition;
        refrenceRotation = transform.localRotation;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (wheel && wheel.vehicle)
        {
            float offset = wheel.onGround ? wheel.contactHit.distance : wheel.SuspensionRestLength;
            transform.localPosition = refrencePosition + transform.parent.up * (wheelRadius - offset);

            float changeDist = Vector3.Dot(transform.parent.forward, wheel.worldVelocity * Time.fixedDeltaTime);
            float changeRoll = (changeDist * 360) / (2 * Mathf.PI * wheelRadius);
            currentRoll += changeRoll;

            transform.localRotation = refrenceRotation;
            transform.Rotate(new Vector3(0, 0, currentRoll), Space.Self);

            if (wheel.effectedBySteer)
            {
                transform.Rotate(transform.parent.up, wheel.vehicle.currentSteer, Space.World);
            }

        }
    }
}
