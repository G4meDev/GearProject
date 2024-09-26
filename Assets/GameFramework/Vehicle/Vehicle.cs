using UnityEngine;
using UnityEngine.UI;

public enum VehicleAeroState
{
    OnGround,
    Coyote,
    Jumping,
    Falling,
    Gliding
}

public class Vehicle : MonoBehaviour
{
    public Rigidbody vehicleProxy;
    public GameObject vehicleBox;
    public GameObject vehicleMesh;

    public UI_ScreenInput screenInput;

    public MeshRenderer boostIndicator;

    public Text speedText;
    public Text tractionText;
    public Text boostText;
    public Text reserveText;

    public Image aeroMeter;
    public Image driftMeter;

    public float gravityStr = 25.0f;
    private Vector3 gravityDir = Vector3.down;

    public float counterForceStr = 0.03f;

    public float maxSpeed = 20.0f;
    public float accel = 20.0f;

    public float rayDist = 1.0f;

    public float speedModifierIntensity;
    public float speedModifierReserveTime;

    public float boostIntensity = 10.0f;

    public float traction = 1.0f;
    public float driftTraction = 0.0f;
    public AnimationCurve steerCurve;
    public float steerVelocityFriction = 2.0f;

    public float jumpStr = 300.0f;

    [HideInInspector]
    public float forwardSpeed = 0.0f;

    readonly float maxSpeedModifier = 20.0f;


    float lastTimeOnGround = 0.0f;

    public float coyoteTime = 0.1f;

    public float jumpDelayTime = 0.1f;

    public float jumpResetTime = 0.4f;

    public float lowJumpTime = 0.6f;
    public SpeedModifierData lowJumpSpeedModifier;

    public float midJumpTime = 1.0f;
    public SpeedModifierData midJumpSpeedModifier;

    public float highJumpTime = 1.4f;
    public SpeedModifierData highJumpSpeedModifier;

    [HideInInspector]
    float lastjumpTime = 0;

    [HideInInspector]
    public float vInput;

    [HideInInspector]
    public float hInput;

    [HideInInspector]
    private bool pressedJump;
    [HideInInspector]
    private bool holdingJump;

    [HideInInspector]
    private RaycastHit hit;

    [HideInInspector]
    private bool bHit;

    [HideInInspector]
    private Vector3 contactSmoothNormal;

    [HideInInspector]
    private float steerValue;

    [HideInInspector]
    private float maxSpeedWithModifier;

    [HideInInspector]
    VehicleAeroState aeroState = VehicleAeroState.OnGround;

    [HideInInspector]
    private float airborneTime = 0.0f;


    public float driftMinAngle = 15.0f;
    public float driftMaxAngle = 60.0f;

    public float driftTimer = 2.0f;

    public float driftTractionRestTime = 2.0f;

    public SpeedModifierData firstDirftSpeedModifier;

    public SpeedModifierData secondDirftSpeedModifier;

    public SpeedModifierData thirdDirftSpeedModifier;

    [HideInInspector]
    public bool drifting = false;

    [HideInInspector]
    private float driftStartTime = 0.0f;

    [HideInInspector]
    private int driftCounter = 0;

    [HideInInspector]
    public float driftYaw = 0.0f;

    [HideInInspector]
    private bool rightDrift = false;

    [HideInInspector]
    private float lastDriftEndTime = 0.0f;

    //TODO: Delete
    public PullPath pullPath;

    public Camera_Orient_Node orientNode;

    public AntiGravity_Node antiGravityNode;

    public Glider_Node gliderNode;

    private void UpdateScreenInput()
    {
        if (screenInput)
        {
            hInput = Mathf.Clamp(hInput + screenInput.data.hInput, -1, 1);
            vInput = Mathf.Clamp(vInput + screenInput.data.vInput, -1, 1);

            pressedJump |= screenInput.data.pressedJump;
            holdingJump |= screenInput.data.holdingJump;
        }
    }

    public void StartGliding(Glider_Node node)
    {
        if (gliderNode == null)
        {
            //vehicleProxy.velocity = Vector3.zero;

        }
        
        gliderNode = node;
        aeroState = VehicleAeroState.Gliding;
    }

    public void EndGliding()
    {
        gliderNode = null;
        aeroState = VehicleAeroState.Falling;
    }

    private void DoGliding()
    {
        Vector3 d = gliderNode.GetDesigeredVelocity(vehicleProxy.transform.position);
        vehicleProxy.velocity = Vector3.Lerp(vehicleProxy.velocity, d, 0.04f);

        Quaternion targetRotation = Quaternion.LookRotation(Vector3.Normalize(gliderNode.next.transform.position - gliderNode.transform.position), vehicleBox.transform.up);

        vehicleBox.transform.rotation = Quaternion.Slerp(vehicleBox.transform.rotation, targetRotation, 0.04f);
    }

    private bool CanJump()
    {
        return (aeroState == VehicleAeroState.OnGround || aeroState == VehicleAeroState.Coyote) && Time.time > lastjumpTime + jumpResetTime;
    }

    private bool CanDrift()
    {
        return holdingJump && Mathf.Abs(hInput) > 0.5f;
    }

    private void StartDrift()
    {
        drifting = true;
        driftStartTime = Time.time;
        driftCounter = 0;
        driftYaw = 0.0f;

        rightDrift = hInput > 0.0f;
    }

    private void StepDrift()
    {
        if(!holdingJump)
        {
            EndDrift();
        }

        else
        {
            if(Time.time > driftStartTime + driftTimer)
            {
                driftCounter++;
                driftStartTime = Time.time;
            
                if (driftCounter == 1)
                {
                    ApplySpeedModifier(ref firstDirftSpeedModifier);
                }
            
                else if(driftCounter == 2)
                {
                    ApplySpeedModifier(ref secondDirftSpeedModifier);
                }
            
                else if(driftCounter == 3)
                {
                    ApplySpeedModifier(ref thirdDirftSpeedModifier);
            
                    //EndDrift();
                }
            }

            float a = (hInput + 1) / 2;

            driftYaw = rightDrift ? Mathf.Lerp(driftMinAngle , driftMaxAngle, a) : -Mathf.Lerp(driftMinAngle, driftMaxAngle, 1 - a);
            driftYaw *= Time.fixedDeltaTime;

            vehicleBox.transform.Rotate(Vector3.up, driftYaw, Space.Self);
        }
    }

    private void EndDrift()
    {
        drifting = false;

        lastDriftEndTime = Time.time;
    }

    private void OnStartJump()
    {
        if (CanJump())
        {
            vehicleProxy.AddForce(jumpStr * contactSmoothNormal, ForceMode.Acceleration);

            aeroState = VehicleAeroState.Jumping;

            lastjumpTime = Time.time;
        }
    }

    private void OnEndJump()
    {
        if (airborneTime > highJumpTime)
        {
            ApplySpeedModifier(ref highJumpSpeedModifier);
        }

        else if (airborneTime > midJumpTime)
        {
            ApplySpeedModifier(ref midJumpSpeedModifier);
        }

        else if (airborneTime > lowJumpTime)
        {
            ApplySpeedModifier(ref lowJumpSpeedModifier);
        }

        if (CanDrift())
        {
            StartDrift();
        }

    }

    private float GetContactSurfaceFriction()
    {
        if (bHit)
        {
            return hit.collider.material.dynamicFriction - 100;
        }

        return 0;
    }

    private float GetContactSurfaceLateralFriction()
    {
        if (bHit)
        {
            return hit.collider.material.staticFriction - 100;
        }

        return 0;
    }

    public void StartFalling()
    {

    }

    void UpdateAeroState()
    {
        if (aeroState == VehicleAeroState.Gliding)
            return;

        if (!bHit)
        {
            if (aeroState != VehicleAeroState.Jumping)
            {
                if (Time.time < lastTimeOnGround + coyoteTime)
                {
                    aeroState = VehicleAeroState.Coyote;
                }
                else if(aeroState == VehicleAeroState.Coyote)
                {
                    aeroState = VehicleAeroState.Falling;
                    StartFalling();
                }
                else
                {
                    aeroState = VehicleAeroState.Falling;
                }
            }

        }

        else
        {
            if (aeroState == VehicleAeroState.Jumping)
            {
                // add delay to let ray get out of surface (ray is longer than vehicle proxy radius)
                if (Time.time > lastjumpTime + jumpDelayTime)
                {
                    aeroState = VehicleAeroState.OnGround;
                    lastTimeOnGround = Time.time;

                    OnEndJump();
                }
            }

            else
            {
                aeroState = VehicleAeroState.OnGround;
                lastTimeOnGround = Time.time;
            }
        }
    }

    public float GetMaxSpeedWithModifiers()
    {
        float modifier = speedModifierIntensity;

        return maxSpeed + modifier - GetContactSurfaceFriction() - (Mathf.Abs(steerValue) * steerVelocityFriction);
    }

    private void UpdateSpeedModifiers()
    {
        speedModifierReserveTime -= Time.fixedDeltaTime;
        if (speedModifierReserveTime < 0)
        {
            speedModifierReserveTime = 0;
            speedModifierIntensity = 0;
        }
    }

    public void ApplySpeedModifier(ref SpeedModifierData data)
    {
        speedModifierReserveTime += data.reserverTime;

        if (speedModifierIntensity < data.intensity)
        {
            speedModifierIntensity = data.intensity;
        }
    }

    public void IncreaseSpeedTo(float targetSpeed)
    {
        if (forwardSpeed < targetSpeed)
        {
            vehicleProxy.AddForce((targetSpeed - forwardSpeed) * vehicleBox.transform.forward, ForceMode.VelocityChange);
        }
    }

    private void RaycastForContactSurface()
    {
        Ray ray = new(vehicleProxy.transform.position, gravityDir);

        // TODO: make track surface and track wall layer
        LayerMask layerMask = LayerMask.GetMask("Default");
        bHit = Physics.Raycast(ray, out hit, rayDist, layerMask);

        if (bHit)
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (meshCollider == null)
            {
                contactSmoothNormal = -gravityDir;
            }
            else
            {
                Mesh mesh = meshCollider.sharedMesh;
                Vector3[] normals = mesh.normals;
                int[] triangles = mesh.triangles;

                Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];

                Vector3 baryCenter = hit.barycentricCoordinate;

                contactSmoothNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
                contactSmoothNormal = contactSmoothNormal.normalized;

                Transform hitTransform = hit.collider.transform;
                contactSmoothNormal = hitTransform.TransformDirection(contactSmoothNormal);

                Debug.DrawRay(hit.point, contactSmoothNormal);
            }

        }

        else
        {
            contactSmoothNormal = -gravityDir;
        }

        Debug.DrawLine(vehicleProxy.transform.position, vehicleProxy.transform.position + gravityDir * rayDist);
    }

    private void Gravity()
    {
        if(antiGravityNode)
        {
            gravityDir = -antiGravityNode.GetUpVector(vehicleProxy.transform.position);
        }

        else
        {
            gravityDir = Vector3.down;
        }

        if (gliderNode)
            return;

        // gravity force
        vehicleProxy.AddForce(gravityDir * gravityStr, ForceMode.Acceleration);
    }

    private void ApplySteer()
    {
        if(gliderNode)
        {
            vehicleProxy.AddForce(100 * hInput * vehicleBox.transform.right, ForceMode.Acceleration);

            return;
        }

        // applying vehicle yaw to the box
        if (!drifting)
        {
            steerValue = steerCurve.Evaluate(vehicleProxy.velocity.magnitude) * hInput * Time.fixedDeltaTime;
            steerValue = forwardSpeed > 0 ? steerValue : -steerValue;
        }
        else
        {
            steerValue = 0;
        }

        vehicleBox.transform.Rotate(Vector3.up, steerValue, Space.Self);
    }

    private void AlignWithContactSurface()
    {
        // if in touch with ground align with surface normal otherwise align with world up 
        Vector3 boxUp = bHit ? contactSmoothNormal : -gravityDir;
        Vector3 nForward = Vector3.Normalize(Vector3.Cross(vehicleBox.transform.right, boxUp));
        Quaternion q = Quaternion.LookRotation(nForward, boxUp);

        vehicleBox.transform.rotation = q;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        //Time.fixedDeltaTime = 0.01f;

        aeroMeter.material.SetFloat("_low", lowJumpTime);
        aeroMeter.material.SetFloat("_mid", midJumpTime);
        aeroMeter.material.SetFloat("_high", highJumpTime);


    }

    private void FixedUpdate()
    {
        vInput = UnityEngine.Input.GetAxis("Vertical");
        hInput = UnityEngine.Input.GetAxis("Horizontal");

        pressedJump = UnityEngine.Input.GetButtonDown("Jump");
        holdingJump = UnityEngine.Input.GetButton("Jump");

        Gravity();

        UpdateScreenInput();

        //bool boosting = UnityEngine.Input.GetButton("Boost");

        UpdateSpeedModifiers();

        float speedModifierAlpha = speedModifierIntensity / maxSpeedModifier;
        boostIndicator.GetComponent<MeshRenderer>().material.SetFloat("_a", speedModifierAlpha);

        boostText.text = string.Format("Boost : {0:F2}", speedModifierIntensity);
        reserveText.text = string.Format("Reserve : {0:F2}", speedModifierReserveTime);

        

        vehicleBox.transform.position = vehicleProxy.transform.position;


        RaycastForContactSurface();

        //Gravity();

        if (drifting)
        {
            StepDrift();
        }

        //forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);
        forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward) > 0 ? vehicleProxy.velocity.magnitude : -vehicleProxy.velocity.magnitude;

        ApplySteer();

        AlignWithContactSurface();

        maxSpeedWithModifier = GetMaxSpeedWithModifiers();

        airborneTime = aeroState == VehicleAeroState.Jumping ? Time.time - lastjumpTime : 0.0f;
        aeroMeter.material.SetFloat("_airborneTime", airborneTime);

        UpdateAeroState();

        //forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);


        driftMeter.material.SetFloat("_Drifting", drifting ? 1 : 0);
        driftMeter.material.SetFloat("_StartTime", driftStartTime);
        driftMeter.material.SetFloat("_Duration", driftTimer);
        driftMeter.material.SetFloat("_Counter", driftCounter);

        if(aeroState == VehicleAeroState.Gliding)
        {
            DoGliding();
        }

        if (!bHit)
        {


        }
        if (bHit && aeroState != VehicleAeroState.Gliding)
        {

            // if boosting set speed to max
            //if (maxSpeed < maxSpeedWithModifier)
            if (speedModifierReserveTime > 0)
            {
                IncreaseSpeedTo(maxSpeedWithModifier);
            }

            float enginePower = Mathf.Abs(forwardSpeed) < maxSpeedWithModifier ? accel : 0;
            enginePower *= vInput;


            vehicleProxy.AddForce(vehicleBox.transform.forward * enginePower, ForceMode.Acceleration);

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : slipingSpeed / vehicleProxy.velocity.magnitude;


            if (Mathf.Abs(slipingSpeedRatio) > 0)
            {
                //float t = drifting ? 0 : Mathf.Clamp01((Time.time - lastDriftEndTime) / driftTractionRestTime);
                //float t = drifting ? driftTraction : traction;

                float t = drifting ? driftTraction : Mathf.Lerp(driftTraction, GetContactSurfaceLateralFriction(), Mathf.Clamp01((Time.time - lastDriftEndTime) / driftTractionRestTime));

                vehicleProxy.AddForce(-slipingSpeed * t * vehicleBox.transform.right, ForceMode.VelocityChange);

                tractionText.text = string.Format("Traction = {0:F2}", traction);
            }

        }


        vehicleProxy.AddForce((vehicleProxy.velocity.magnitude == 0 ? 0 : counterForceStr) * -vehicleProxy.velocity.normalized, ForceMode.VelocityChange);

        if (pressedJump)
        {
            OnStartJump();
        }

        speedText.text = string.Format("Speed : {0:F2}", forwardSpeed);


//         if (bHit && pullPath && forwardSpeed < maxSpeedWithModifier)
//         {
//             Vector3 tan = pullPath.GetForceAtPosition(vehicleProxy.transform.position);
// 
//             Debug.DrawLine(vehicleProxy.transform.position + Vector3.up * 2, vehicleProxy.transform.position + (Vector3.up * 2) + Vector3.Normalize(tan), Color.black);
// 
//             vehicleProxy.AddForce(tan, ForceMode.Acceleration);
//         }
    }
}
