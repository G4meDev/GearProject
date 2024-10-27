using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// @TODO: Develop jump

public enum VehicleAeroState
{
    OnGround,
    Coyote,
    Jumping,
    Falling
}

public class Vehicle : MonoBehaviour
{
    public bool isPlayer = true;

    public GameObject cameraPrefab;

    public Rigidbody vehicleProxy;
    public GameObject vehicleBox;
    public GameObject vehicleMesh;

    //[HideInInspector]
    public int position = -1;

    //TODO: Remove
    public MeshRenderer boostIndicator;

    public float gravityStr = 25.0f;
    private Vector3 gravityDir = Vector3.down;

    public float counterForceStr = 0.03f;

    public float maxSpeed = 20.0f;
    public float maxSpeedReverse = -10.0f;
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

    public float jumpDuration = 0.1f;

    public float lowJumpTime = 0.6f;
    public SpeedModifierData lowJumpSpeedModifier;

    public float midJumpTime = 1.0f;
    public SpeedModifierData midJumpSpeedModifier;

    public float highJumpTime = 1.4f;
    public SpeedModifierData highJumpSpeedModifier;

    [HideInInspector]
    public float jumpStartTime = 0;

    [HideInInspector]
    public float vInput;

    [HideInInspector]
    public float hInput;

    [HideInInspector]
    public bool holdingJump;

    [HideInInspector]
    private RaycastHit hit;

    [HideInInspector]
    private bool bHit;

    [HideInInspector]
    private Vector3 contactSmoothNormal;

    [HideInInspector]
    public float steerValue;

    [HideInInspector]
    private float maxSpeedWithModifier;

    [HideInInspector]
    public VehicleAeroState aeroState = VehicleAeroState.OnGround;

    [HideInInspector]
    public float airborneTime = 0.0f;

    public float minDriftSpeed = 20.0f;
    public float driftMinAngle = 15.0f;
    public float driftMaxAngle = 60.0f;

    public float driftTimer = 2.0f;

    public float driftTractionRestTime = 2.0f;

    public SpeedModifierData firstDirftSpeedModifier;

    public SpeedModifierData secondDirftSpeedModifier;

    public SpeedModifierData thirdDirftSpeedModifier;


    [HideInInspector]
    public float driftStartTime = 0.0f;

    [HideInInspector]
    public int driftCounter = 0;

    [HideInInspector]
    public float driftYaw = 0.0f;

    [HideInInspector]
    private float driftDir = 0;

    [HideInInspector]
    private float lastDriftEndTime = 0.0f;

    public Camera_Orient_Node orientNode;

    public AntiGravity_Node antiGravityNode;

    public Glider_Node gliderNode;

    public LapPath_Node lapPathNode;
    public float distanceFromStart = -1;
    public int currentLap = 1;

    public float distanceFromFirstPlace = -1;

    public delegate void KillDelegate();
    public KillDelegate killDelegate;


    public void SetThrottleInput(float input)
    {
        vInput = Mathf.Clamp(input, -1, 1);
    }

    public void SetSteerInput(float input)
    {
        hInput = Mathf.Clamp(input, -1, 1);
    }

    public void EndRace()
    {
        Debug.Log(name + "    End Race!");
    }

    public void IncreaseLap()
    {
        currentLap++;

        if (currentLap > SceneManager.lapCount)
        {
            EndRace();
        }

        if (isPlayer)
        {
            SceneManager.GetLapCounter().UpdateLapCounter();
        }
    }

    public void OnEnterLapPath(LapPath_Node node)
    {
        if (lapPathNode == null)
        {
            lapPathNode = node;
        }

        else if (node.isStart)
        {
            if(lapPathNode.checkpoint == LapPathCheckPoint.checkpoint_3)
            {
                lapPathNode = node;
                IncreaseLap();
            }
        }

        else if (lapPathNode.checkpoint == node.checkpoint || lapPathNode.checkpoint + 1 == node.checkpoint)
        {
            lapPathNode = node;
        }
    }

    public void UpdateLapPathIndex()
    {
        if (lapPathNode)
        {
            distanceFromStart = lapPathNode.GetDistanceFromStart(this);
        }
    }

    public void OnKilled()
    {
        vehicleProxy.velocity = Vector3.zero;
        vehicleProxy.angularVelocity = Vector3.zero;

        speedModifierIntensity = 0.0f;
        speedModifierReserveTime = 0.0f;

        Ray ray = new(lapPathNode.spawnPoint.transform.position + Vector3.up * 2, Vector3.down);
        LayerMask layerMask = LayerMask.GetMask("Default");
        bool bhit = Physics.Raycast(ray, out hit, 5, layerMask);

        Vector3 targetPos = bhit ? hit.point + Vector3.up * 0.65f : lapPathNode.spawnPoint.transform.position;

        vehicleProxy.MovePosition(targetPos);
        vehicleBox.transform.SetPositionAndRotation(targetPos, lapPathNode.spawnPoint.transform.rotation);
        vehicleMesh.transform.SetPositionAndRotation(targetPos, lapPathNode.spawnPoint.transform.rotation);

        killDelegate?.Invoke();
    }

    private bool CanJump()
    {
        return (aeroState == VehicleAeroState.OnGround || aeroState == VehicleAeroState.Coyote);
    }

    public bool isDrifting()
    {
        return driftDir != 0;
    }

    private bool CanDrift()
    {
        return holdingJump && Mathf.Abs(hInput) > 0.5f && forwardSpeed > minDriftSpeed;
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
                if (Time.time > jumpStartTime + jumpDuration)
                {
                    aeroState = VehicleAeroState.OnGround;
                    lastTimeOnGround = Time.time;
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

        // @TODO: find faster way
        //contactSmoothNormal = bHit ? MeshHelpers.GetSmoothNormalFromHit(ref hit) : -gravityDir;
        contactSmoothNormal = bHit ? hit.normal : -gravityDir;
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
        // applying vehicle yaw to the box
        if (!isDrifting() && aeroState == VehicleAeroState.OnGround)
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
        if(isPlayer)
        {
            if(cameraPrefab)
            {
                GameObject obj = Instantiate(cameraPrefab);
                obj.transform.parent = this.transform;
                obj.transform.position = transform.position;
            }

            gameObject.AddComponent<PlayerInput>();

            SceneManager.OnPlayerChanged(this);
        }

        else
        {
            gameObject.AddComponent<AIRoutePlanning>();
            gameObject.AddComponent<AIController>();
        }
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        UpdateLapPathIndex();

        distanceFromFirstPlace = SceneManager.GetDistanceFromFirstPlace(this);

        Gravity();

        UpdateSpeedModifiers();

        float speedModifierAlpha = speedModifierIntensity / maxSpeedModifier;
        boostIndicator.GetComponent<MeshRenderer>().material.SetFloat("_a", speedModifierAlpha);

        vehicleBox.transform.position = vehicleProxy.transform.position;


        RaycastForContactSurface();

        //Gravity();

        //forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);
        forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward) > 0 ? vehicleProxy.velocity.magnitude : -vehicleProxy.velocity.magnitude;

        ApplySteer();

        AlignWithContactSurface();

        maxSpeedWithModifier = GetMaxSpeedWithModifiers();

        airborneTime = aeroState == VehicleAeroState.Jumping ? Time.time - jumpStartTime : 0.0f;

        UpdateAeroState();

        if (!bHit)
        {


        }
        if (bHit)
        {

            // if boosting set speed to max
            //if (maxSpeed < maxSpeedWithModifier)
            if (speedModifierReserveTime > 0)
            {
                IncreaseSpeedTo(maxSpeedWithModifier);
            }

            float enginePower;

            if (vInput > 0)
            {
                enginePower = forwardSpeed < maxSpeedWithModifier ? accel : 0;
            }

            else
            {
                enginePower = forwardSpeed > maxSpeedReverse ? accel : 0;
            }


            enginePower *= vInput;

            if(aeroState == VehicleAeroState.OnGround)
            {
                vehicleProxy.AddForce(vehicleBox.transform.forward * enginePower, ForceMode.Acceleration);
            }

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : slipingSpeed / vehicleProxy.velocity.magnitude;


            if (Mathf.Abs(slipingSpeedRatio) > 0)
            {
                //float t = drifting ? 0 : Mathf.Clamp01((Time.time - lastDriftEndTime) / driftTractionRestTime);
                //float t = drifting ? driftTraction : traction;

                float t = isDrifting() ? driftTraction : Mathf.Lerp(driftTraction, GetContactSurfaceLateralFriction(), Mathf.Clamp01((Time.time - lastDriftEndTime) / driftTractionRestTime));

                vehicleProxy.AddForce(-slipingSpeed * t * vehicleBox.transform.right, ForceMode.VelocityChange);
            }

        }

        vehicleProxy.AddForce((vehicleProxy.velocity.magnitude == 0 ? 0 : counterForceStr) * -vehicleProxy.velocity.normalized, ForceMode.VelocityChange);
    }
}
