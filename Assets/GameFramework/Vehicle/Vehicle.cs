using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

// @TODO: Develop jump

public enum VehicleAeroState
{
    OnGround,
    Coyote,
    Jumping,
    Falling,
    Gliding
}

public class Vehicle : Agent
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

    public float minDriftSpeed = 30.0f;
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
    public float lapPathIndex = -1;
    public int currentLap = 1;

    public float distanceFromFirstPlace = -1;

    public delegate void KillDelegate();
    public KillDelegate killDelegate;

    public float maxPossibleSpeed = 55.0f;

    // ------------------------------------------------------------------
    [NonSerialized]
    public AIRoutePlanning routePlanning;

    public Image rewardImage;

    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;

    public LinkedList<AI_Route_Node> aiRoutePath = new();

    Vector3 startPos = Vector3.zero;
    Quaternion startRot = Quaternion.identity;

    public float targetSpeed = 30.0f;

    private Vector3 crossTrackLocal = Vector2.zero;
    private float crossTrackScale = 20.0f;

    private Vector3 p_1_Local = Vector2.zero;
    private float p_1_Scale = 20.0f;

    private Vector3 p_2_Local = Vector2.zero;
    private float p_2_Scale = 20.0f;

    private Vector3 p_3_Local = Vector2.zero;
    private float p_3_Scale = 20.0f;

    private float changedDist = 0.0f;

    private float localDivide = AI_Params.projection_3_dist * 1.5f;
    private float scaleDivide = 60.0f;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(forwardSpeed/ maxPossibleSpeed);
        sensor.AddObservation(hInput);
        sensor.AddObservation(driftDir);
        sensor.AddObservation(isDrifting() ? Mathf.Clamp01((Time.time - driftStartTime) / 6) : 0);
        sensor.AddObservation(targetSpeed/ maxPossibleSpeed);


        sensor.AddObservation(crossTrackLocal / localDivide);
        sensor.AddObservation(crossTrackScale / scaleDivide);

        sensor.AddObservation(p_1_Local / localDivide);
        sensor.AddObservation(p_1_Scale / scaleDivide);

        sensor.AddObservation(p_2_Local / localDivide);
        sensor.AddObservation(p_2_Scale / scaleDivide);


        sensor.AddObservation(p_3_Local / localDivide);
        sensor.AddObservation(p_3_Scale / scaleDivide);

        //         sensor.AddObservation(dot);
        //         sensor.AddObservation((roadWidth - dist) / 40);
        //         sensor.AddObservation((roadWidth + dist) / 40);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        hInput = actions.ContinuousActions[0];
        vInput = actions.ContinuousActions[1];

        bool jump = actions.DiscreteActions[0] == 1;
        holdingJump = jump;


        if (routePlanning.projectionData != null && crossTrackLocal.magnitude > crossTrackScale / 2)
        {
            OnKilled();
        }

        float speedReward = 1 - (Mathf.Clamp(forwardSpeed, 0, float.MaxValue) - targetSpeed) / maxPossibleSpeed;
        speedReward *= changedDist * 0.7f;

        float constantDec = -0.005f;

        float reward = speedReward + constantDec;

        if(isPlayer)
        {
            //Debug.Log(reward + "    " + speedReward + "    " + targetSpeed);
        }

        rewardImage.material.SetFloat("_reward", reward);

        SetReward(reward);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.ContinuousActions;
        var b = actionsOut.DiscreteActions;

        PlayerInput i = GetComponent<PlayerInput>();

        a[0] = i.hInput;
        a[1] = i.vInput;

        b[0] = i.holdingJump ? 1 : 0;
    }
    public override void Initialize()
    {
        
    }
    public override void OnEpisodeBegin()
    {
        vehicleProxy.velocity = Vector3.zero;
        vehicleProxy.angularVelocity = Vector3.zero;

        //targetSpeed = Random.Range(30, 55);

        //lapPathNode = null;
        //aiRouteNode_Current = null;
        //aiRouteNode_Target = null;
        lapPathIndex = 0;
        currentLap = 1;

        EndDrift();
        
        speedModifierIntensity = 0.0f;
        speedModifierReserveTime = 0.0f;

        //         Vector3 targetPos;
        //         Quaternion q;
        // 
        //         if(lapPathNode)
        //         {
        //             Ray ray = new(lapPathNode.spawnPoint.transform.position + Vector3.up * 2, Vector3.down);
        //             LayerMask layerMask = LayerMask.GetMask("Default");
        //             bool bhit = Physics.Raycast(ray, out hit, 5, layerMask);
        // 
        //             targetPos = bhit ? hit.point + Vector3.up * 0.65f : lapPathNode.spawnPoint.transform.position;
        //             q = lapPathNode.spawnPoint.transform.rotation;
        //         }
        //         else
        //         {
        //             targetPos = vehicleProxy.transform.position;
        //             q = vehicleProxy.transform.rotation;
        //         }

        

        vehicleProxy.MovePosition(startPos);
        vehicleBox.transform.SetPositionAndRotation(startPos, startRot);
        vehicleMesh.transform.SetPositionAndRotation(startPos, startRot);
        
        killDelegate?.Invoke();
    }

    private void UpdateRoute()
    {
        if (routePlanning.projectionData != null)
        {
            Vector3 crossTrackPos = Vector3.Lerp(routePlanning.projectionData.crossTrackProjection.parent.transform.position,
                routePlanning.projectionData.crossTrackProjection.child.transform.position,
                routePlanning.projectionData.crossTrackProjection.t);

            crossTrackLocal = vehicleBox.transform.InverseTransformPointUnscaled(crossTrackPos);

            crossTrackScale = Mathf.Lerp(routePlanning.projectionData.crossTrackProjection.parent.transform.lossyScale.x,
                routePlanning.projectionData.crossTrackProjection.child.transform.lossyScale.x,
                routePlanning.projectionData.crossTrackProjection.t);

            // -------------------------------------------------------------------------------------------------------------------------------------------

            Vector3 projection_1_Pos = Vector3.Lerp(routePlanning.projectionData.Projection_1.parent.transform.position,
                routePlanning.projectionData.Projection_1.child.transform.position,
                routePlanning.projectionData.Projection_1.t);

            p_1_Local = vehicleBox.transform.InverseTransformPointUnscaled(projection_1_Pos);

            p_1_Scale = Mathf.Lerp(routePlanning.projectionData.Projection_1.parent.transform.lossyScale.x,
                routePlanning.projectionData.Projection_1.child.transform.lossyScale.x,
                routePlanning.projectionData.Projection_1.t);

            // -------------------------------------------------------------------------------------------------------------------------------------------

            Vector3 projection_2_Pos = Vector3.Lerp(routePlanning.projectionData.Projection_2.parent.transform.position,
                routePlanning.projectionData.Projection_2.child.transform.position,
                routePlanning.projectionData.Projection_2.t);

            p_2_Local = vehicleBox.transform.InverseTransformPointUnscaled(projection_2_Pos);

            p_2_Scale = Mathf.Lerp(routePlanning.projectionData.Projection_2.parent.transform.lossyScale.x,
                routePlanning.projectionData.Projection_2.child.transform.lossyScale.x,
                routePlanning.projectionData.Projection_2.t);

            // -------------------------------------------------------------------------------------------------------------------------------------------

            Vector3 projection_3_Pos = Vector3.Lerp(routePlanning.projectionData.Projection_3.parent.transform.position,
                routePlanning.projectionData.Projection_3.child.transform.position,
                routePlanning.projectionData.Projection_3.t);

            p_3_Local = vehicleBox.transform.InverseTransformPointUnscaled(projection_3_Pos);

            p_3_Scale = Mathf.Lerp(routePlanning.projectionData.Projection_3.parent.transform.lossyScale.x,
                routePlanning.projectionData.Projection_3.child.transform.lossyScale.x,
                routePlanning.projectionData.Projection_3.t);


            changedDist = routePlanning.projectionData.changedDist;

#if UNITY_EDITOR

            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(name.GetHashCode());
            Color color = UnityEngine.Random.ColorHSV();
            UnityEngine.Random.state = state;

            DrawHelpers.DrawSphere(crossTrackPos, 3, color);
            DrawHelpers.DrawSphere(projection_1_Pos, 3, color);
            DrawHelpers.DrawSphere(projection_2_Pos, 3, color);
            DrawHelpers.DrawSphere(projection_3_Pos, 3, color);

//             DrawHelpers.DrawSphere(new Vector3(crossTrackLocal.x, 0, crossTrackLocal.y) * AI_Params.projection_3_dist, 3, color);
//             DrawHelpers.DrawSphere(new Vector3(p_1_Local.x, 0, p_1_Local.y) * AI_Params.projection_3_dist, 3, color);
//             DrawHelpers.DrawSphere(new Vector3(p_2_Local.x, 0, p_2_Local.y) * AI_Params.projection_3_dist, 3, color);
//             DrawHelpers.DrawSphere(new Vector3(p_3_Local.x, 0, p_3_Local.y) * AI_Params.projection_3_dist, 3, color);

#endif
        }

        else
        {
            crossTrackLocal = Vector2.zero;
            crossTrackScale = 20.0f;

            p_1_Local = Vector2.zero;
            p_1_Scale = 20.0f;

            p_2_Local = Vector2.zero;
            p_2_Scale = 20.0f;

            p_3_Local = Vector2.zero;
            p_3_Scale = 20.0f;

            changedDist = 0.0f;
        }
    }

    // ------------------------------------------------------------------

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

        //SetReward(1);
        EndEpisode();
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

            //SetReward(speedReward);
        }
    }

    public void UpdateLapPathIndex()
    {
        if (lapPathNode)
        {
            lapPathIndex = lapPathNode.GetIndexAtWorldPosition(vehicleProxy.transform.position);
        }
    }

    public void OnKilled()
    {
        SetReward(-1);

        EndEpisode();

//         vehicleProxy.velocity = Vector3.zero;
//         vehicleProxy.angularVelocity = Vector3.zero;
// 
//         EndDrift();
// 
//         speedModifierIntensity = 0.0f;
//         speedModifierReserveTime = 0.0f;
// 
//         Ray ray = new(lapPathNode.spawnPoint.transform.position + Vector3.up * 2, Vector3.down);
//         LayerMask layerMask = LayerMask.GetMask("Default");
//         bool bhit = Physics.Raycast(ray, out hit, 5, layerMask);
// 
//         Vector3 targetPos = bhit ? hit.point + Vector3.up * 0.65f : lapPathNode.spawnPoint.transform.position;
// 
//         vehicleProxy.MovePosition(targetPos);
//         vehicleBox.transform.SetPositionAndRotation(targetPos, lapPathNode.spawnPoint.transform.rotation);
//         vehicleMesh.transform.SetPositionAndRotation(targetPos, lapPathNode.spawnPoint.transform.rotation);
// 
//         killDelegate?.Invoke();
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

    public void StartDrift()
    {
        driftDir = Mathf.Sign(hInput);
        
        driftStartTime = Time.time;
        driftCounter = 0;
        driftYaw = 0.0f;
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

            driftYaw = driftDir > 0 ? Mathf.Lerp(driftMinAngle , driftMaxAngle, a) : -Mathf.Lerp(driftMinAngle, driftMaxAngle, 1 - a);
            driftYaw *= Time.fixedDeltaTime;

            vehicleBox.transform.Rotate(Vector3.up, driftYaw, Space.Self);
        }
    }

    public void EndDrift()
    {
        driftDir = 0;

        lastDriftEndTime = Time.time;
    }

    private void OnStartJump()
    {
        if (CanJump())
        {
            //vehicleProxy.AddForce(jumpStr * contactSmoothNormal, ForceMode.Acceleration);

            aeroState = VehicleAeroState.Jumping;

            jumpStartTime = Time.time;
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

//         if (CanDrift())
//         {
//             StartDrift();
//         }

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
                if (Time.time > jumpStartTime + jumpDuration)
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

        contactSmoothNormal = bHit ? MeshHelpers.GetSmoothNormalFromHit(ref hit) : -gravityDir;
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
        routePlanning = GetComponent<AIRoutePlanning>();

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
            //gameObject.AddComponent<AIController>();
        }

        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void Update()
    {
        //distanceFromFirstPlace = SceneManager.GetDistanceFromFirstPlace(this);
    }

    private void FixedUpdate()
    {
        UpdateLapPathIndex();

        Gravity();

        UpdateSpeedModifiers();

        float speedModifierAlpha = speedModifierIntensity / maxSpeedModifier;
        boostIndicator.GetComponent<MeshRenderer>().material.SetFloat("_a", speedModifierAlpha);

        vehicleBox.transform.position = vehicleProxy.transform.position;


        RaycastForContactSurface();

        //Gravity();

        if (isDrifting())
        {
            StepDrift();
        }
        else if (CanDrift())
        {
            StartDrift();
        }

        //forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward);
        forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward) > 0 ? vehicleProxy.velocity.magnitude : -vehicleProxy.velocity.magnitude;

        ApplySteer();

        AlignWithContactSurface();

        maxSpeedWithModifier = GetMaxSpeedWithModifiers();

        airborneTime = aeroState == VehicleAeroState.Jumping ? Time.time - jumpStartTime : 0.0f;

        UpdateAeroState();

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


        UpdateRoute();

    }
}
