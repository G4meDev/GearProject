using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

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
    public bool pressedJump;
    [HideInInspector]
    public bool holdingJump;

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
    public bool drifting = false;

    [HideInInspector]
    public float driftStartTime = 0.0f;

    [HideInInspector]
    public int driftCounter = 0;

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

    public LapPath_Node lapPathNode;
    public float lapPathIndex = -1;
    public int currentLap = 1;

    private Vector3 lastRight;
    private Vector3 lastPos;

    public float distanceFromFirstPlace = -1;

    public delegate void KillDelegate();
    public KillDelegate killDelegate;

    // ------------------------------------------------------------------

    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;

    float roadWidth = 0;
    Vector3 tan = Vector3.zero;

    float lapIndexChange = 0.0f;

    Vector3 startPos = Vector3.zero;
    Quaternion startRot = Quaternion.identity;

    float targetSpeed = 45.0f;

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 nearestpos = GetNearestWorldPosition(out roadWidth, out tan);
        float dist = Vector3.Distance(nearestpos, vehicleProxy.transform.position);
        
        //fix
        Vector3 right = aiRouteNode_Current && aiRouteNode_Target ? Vector3.Cross(Vector3.up, aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position) : Vector3.zero;
        dist = Vector3.Dot(right, vehicleProxy.transform.position - nearestpos) > 0 ? dist : -dist;

        float targetDist = aiRouteNode_Target ? Vector3.Distance(vehicleProxy.transform.position, aiRouteNode_Target.transform.position) : 0;

        float dot = Vector3.Dot(tan, vehicleBox.transform.forward);

        

        sensor.AddObservation(forwardSpeed/maxSpeed);
        sensor.AddObservation(targetSpeed/maxSpeed);
        sensor.AddObservation(dot);
        sensor.AddObservation(dist / 40);
        //sensor.AddObservation(roadWidth / 40);
        sensor.AddObservation((roadWidth - dist) / 40);
        sensor.AddObservation((roadWidth + dist) / 40);
        sensor.AddObservation(targetDist/10);

        sensor.AddOneHotObservation(((int)aeroState), 4);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        hInput = actions.ContinuousActions[0];
        vInput = actions.ContinuousActions[1];

        bool jump = actions.DiscreteActions[0] == 1;
        pressedJump = jump && !holdingJump;
        holdingJump = jump;

        float speedReward = Mathf.Clamp(Mathf.InverseLerp(0, targetSpeed, forwardSpeed), -1, 1);
        speedReward *= 0.1f;

        float indexChangeReward = lapIndexChange * 1;

        float constantDec = -0.001f;
        float jumpDec = pressedJump ? -0.001f : 0;

        float reward = speedReward + constantDec + jumpDec;
        //Debug.Log(reward + "    " + speedReward + "    " + indexChangeReward);

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


    public void UpdateTargetNode()
    {
        if (aiRouteNode_Current.children.Count > 0)
        {
            aiRouteNode_Target = aiRouteNode_Current.children[0];

            //             float targetScale = aiRouteNode_Target.transform.lossyScale.x / 8;
            //             targetTrackError = Random.Range(-targetScale, targetScale);
        }

        else
        {
            aiRouteNode_Target = null;
        }
    }


    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
        aiRouteNode_Current = node;

        UpdateTargetNode();

    }

    public Vector3 GetNearestWorldPosition(out float trackWidth, out Vector3 tan)
    {
        trackWidth = 20.0f;
        tan = Vector3.zero;

        if (aiRouteNode_Current && aiRouteNode_Target)
        {
            Vector3 d = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
            Vector3 toPos = vehicleProxy.transform.position - aiRouteNode_Current.transform.position;

            float dot = Vector3.Dot(d.normalized, toPos);

            // passed target without hitting collision
            if (dot > d.magnitude)
            {
                OnEnterNewRouteNode(aiRouteNode_Target);

                return GetNearestWorldPosition(out trackWidth, out tan);
            }

            // should check for parent nodes
            else if (dot < 0)
            {
                AI_Route_Node bestParent = null;
                float min_dist = float.MaxValue;

                foreach (AI_Route_Node parent in aiRouteNode_Current.parents)
                {
                    float dist = Vector3.Distance(vehicleProxy.transform.position, parent.transform.position);

                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        bestParent = parent;
                    }
                }



                d = aiRouteNode_Current.transform.position - bestParent.transform.position;
                toPos = vehicleProxy.transform.position - bestParent.transform.position;

                dot = Vector3.Dot(d.normalized, toPos);
                dot = Mathf.Clamp(dot, 0, d.magnitude);

                float a = dot / d.magnitude;
                trackWidth = Mathf.Lerp(bestParent.transform.lossyScale.x, aiRouteNode_Current.transform.lossyScale.x, a);
                tan = Vector3.Lerp(bestParent.transform.forward, aiRouteNode_Current.transform.forward, a);

                return bestParent.transform.position + dot * d.normalized;
            }

            else
            {
                float a = dot / d.magnitude;

                trackWidth = Mathf.Lerp(aiRouteNode_Current.transform.lossyScale.x, aiRouteNode_Target.transform.lossyScale.x, a);
                tan = Vector3.Lerp(aiRouteNode_Current.transform.forward, aiRouteNode_Target.transform.forward, a);

                return aiRouteNode_Current.transform.position + dot * d.normalized;
            }
        }

        return Vector3.zero;
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
    }

    public void IncreaseLap()
    {
        currentLap++;

        SetReward(1);
        EndEpisode();

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

            SetReward(0.3f);
        }
    }

    public void UpdateLapPathIndex()
    {
        if (lapPathNode)
        {
            float a = lapPathNode.GetIndexAtWorldPosition(vehicleProxy.transform.position);
            lapIndexChange = a - lapPathIndex;
            lapPathIndex = a;
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
        return (aeroState == VehicleAeroState.OnGround || aeroState == VehicleAeroState.Coyote) && Time.time > lastjumpTime + jumpResetTime;
    }

    private bool CanDrift()
    {
        return holdingJump && Mathf.Abs(hInput) > 0.5f && forwardSpeed > minDriftSpeed;
    }

    public void StartDrift()
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

    public void EndDrift()
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

            float enginePower = Mathf.Abs(forwardSpeed) < maxSpeedWithModifier ? accel : 0;
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

                float t = drifting ? driftTraction : Mathf.Lerp(driftTraction, GetContactSurfaceLateralFriction(), Mathf.Clamp01((Time.time - lastDriftEndTime) / driftTractionRestTime));

                vehicleProxy.AddForce(-slipingSpeed * t * vehicleBox.transform.right, ForceMode.VelocityChange);
            }

        }

        vehicleProxy.AddForce((vehicleProxy.velocity.magnitude == 0 ? 0 : counterForceStr) * -vehicleProxy.velocity.normalized, ForceMode.VelocityChange);

        if (pressedJump)
        {
            OnStartJump();
        }

        Vector3 origin;
        VectorHelpers.LineLineIntersection(out origin, vehicleBox.transform.position, vehicleBox.transform.right,
            lastPos, lastRight);

        //DrawHelpers.DrawSphere(origin, 5, Color.black);
        //Debug.Log(Vector3.Distance(origin, lastPos));

        lastPos = vehicleBox.transform.position;
        lastRight = vehicleBox.transform.right;

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
