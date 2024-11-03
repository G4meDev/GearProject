using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditorInternal;
using UnityEngine;

// @TODO: Develop jump

public enum VehicleAeroState
{
    OnGround,
    Coyote,
    Jumping,
    Falling
}

public class VehicleState : INetworkSerializable
{
    public VehicleState(Vector3 proxy_pos, Quaternion proxy_rot, Vector3 proxy_velo, Vector3 proxy_angularVelo, Vector3 box_pos, Quaternion box_rot)
    {
        vehicleProxy_Position = proxy_pos;
        vehicleProxy_Rotation = proxy_rot;
        vehicleProxy_Velocity = proxy_velo;
        vehicleProxy_AngularVelocity = proxy_angularVelo;

        vehicleBox_Position = box_pos;
        vehicleBox_Rotation = box_rot;
    }

    public VehicleState() : this(Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero, Vector3.zero, Quaternion.identity)
    {

    }

    public Vector3 vehicleProxy_Position;
    public Quaternion vehicleProxy_Rotation;
    public Vector3 vehicleProxy_Velocity;
    public Vector3 vehicleProxy_AngularVelocity;

    public Vector3 vehicleBox_Position;
    public Quaternion vehicleBox_Rotation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref vehicleProxy_Position);
        serializer.SerializeValue(ref vehicleProxy_Rotation);
        serializer.SerializeValue(ref vehicleProxy_Velocity);
        serializer.SerializeValue(ref vehicleProxy_AngularVelocity);

        serializer.SerializeValue(ref vehicleBox_Position);
        serializer.SerializeValue(ref vehicleBox_Rotation);
    }
}

public class VehicleInput : INetworkSerializable
{
    public VehicleInput(float h, float v)
    {
        hInput = h;
        vInput = v;
    }

    public VehicleInput() : this(0, 0)
    {

    }

    public float hInput;
    public float vInput;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hInput);
        serializer.SerializeValue(ref vInput);
    }
}

public class VehicleTimeStamp
{
    public VehicleTimeStamp() : this(0, new VehicleInput(), new VehicleState())
    {

    }

    public VehicleTimeStamp(int frameNum, VehicleInput input, VehicleState state)
    {
        frameNumber = frameNum;

        vehicleInput = input;
        vehicleState = state;
    }

    public int frameNumber;

    public VehicleInput vehicleInput;
    public VehicleState vehicleState;
}

public class Vehicle : NetworkBehaviour
{
    private int clientFrameAheadAmount = 5;

    private int currentFrameNumber = 0;
    private int lastSyncedFrameNumber = 0;

    private CircularBuffer<VehicleTimeStamp> vehicleTimeStamp = new(1024);

    public bool isPlayer = true;

    public GameObject cameraPrefab;

    public Rigidbody vehicleProxy;
    public GameObject vehicleBox;
    public GameObject vehicleMesh;

    public int position = 1;

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

    public AnimationCurve tractionCurve;

    public AnimationCurve steerCurve;
    public float steerVelocityFriction = 2.0f;

    [HideInInspector]
    public float forwardSpeed = 0.0f;

    float lastTimeOnGround = 0.0f;

    public float coyoteTime = 0.1f;

    public float jumpDuration = 0.1f;

    [HideInInspector]
    public float jumpStartTime = 0;

    public float vInput;

    public float hInput;

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

    public AntiGravity_Node antiGravityNode;

    public LapPath_Node lapPathNode;
    public float distanceFromStart = -1;
    public int currentLap = 1;

    public float distanceFromFirstPlace = -1;

    public delegate void KillDelegate();
    public KillDelegate killDelegate;

//     [Rpc(SendTo.Server)]
//     public void SetThrottleInputRpc(float input)
//     {
//         vInput = Mathf.Clamp(input, -1, 1);
//     }
// 
//     [Rpc(SendTo.Server)]
//     public void SetSteerInputRpc(float input)
//     {
//         hInput = Mathf.Clamp(input, -1, 1);
//     }

    public void EndRace()
    {
        Debug.Log(name + "    End Race!");

        if(IsHost)
        {
            //NetworkManager.Shutdown();
        }
    }

    public void IncreaseLap()
    {
        currentLap++;

        if (currentLap > SceneManager.Get().lapCount)
        {
            EndRace();
        }

        if (isPlayer)
        {
            SceneManager.Get().lapCounter.UpdateLapCounter();
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
            if (lapPathNode.checkpoint == LapPathCheckPoint.checkpoint_3)
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
                else if (aeroState == VehicleAeroState.Coyote)
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
        if (antiGravityNode)
        {
            gravityDir = -antiGravityNode.GetUpVector(vehicleProxy.transform.position);
        }

        else
        {
            gravityDir = Vector3.down;
        }

        // gravity force
        vehicleProxy.AddForce(gravityDir * gravityStr, ForceMode.Acceleration);
    }

    private void ApplySteer()
    {
        // applying vehicle yaw to the box
        if (aeroState == VehicleAeroState.OnGround)
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

    public void Init()
    {
        if (IsServer && !isPlayer)
        {
            gameObject.AddComponent<AIRoutePlanning>();
            gameObject.AddComponent<AIController>();
        }

        if (isPlayer && IsOwner)
        {
            if (cameraPrefab)
            {
                GameObject obj = Instantiate(cameraPrefab);
                obj.transform.parent = transform;
                obj.transform.position = transform.position;
            }

            gameObject.AddComponent<PlayerInput>();
        }

        if (!IsHost)
        {
            currentFrameNumber = clientFrameAheadAmount;
        }
    }

    public override void OnNetworkSpawn()
    {
        Init();

        SceneManager.Get().RegisterVehicle(this);
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.Get().UnregisterVehicle(this);
    }

    void Start()
    {

    }

    private void Update()
    {

    }
    
    public void StepVehicleMovement()
    {
        vehicleBox.transform.position = vehicleProxy.position;
        Physics.SyncTransforms();

        forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.forward) > 0 ? vehicleProxy.velocity.magnitude : -vehicleProxy.velocity.magnitude;
        
        Gravity();
        UpdateSpeedModifiers();
        maxSpeedWithModifier = GetMaxSpeedWithModifiers();
        
        RaycastForContactSurface();
        ApplySteer();
        AlignWithContactSurface();
        UpdateAeroState();

        if (!bHit)
        {


        }
        if (bHit)
        {
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

            if (aeroState == VehicleAeroState.OnGround)
            {
                vehicleProxy.AddForce(vehicleBox.transform.forward * enginePower, ForceMode.Acceleration);
            }

            float slipingSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleBox.transform.right);
            float slipingSpeedRatio = vehicleProxy.velocity.magnitude == 0 ? 0 : Mathf.Abs(slipingSpeed) / vehicleProxy.velocity.magnitude;

            //float t = GetContactSurfaceLateralFriction();

            float traction = Mathf.Clamp01(tractionCurve.Evaluate(slipingSpeedRatio));
            //Debug.Log("slip:" + slipingSpeedRatio + "    traction:" + traction);

            vehicleProxy.AddForce(-slipingSpeed * traction * vehicleBox.transform.right, ForceMode.VelocityChange);

        }

        vehicleProxy.AddForce((vehicleProxy.velocity.magnitude == 0 ? 0 : counterForceStr) * -vehicleProxy.velocity.normalized, ForceMode.VelocityChange);
    }

    VehicleState MakeVehicleState()
    {
        return new VehicleState(
            vehicleProxy.position,
            vehicleProxy.rotation,
            vehicleProxy.velocity,
            vehicleProxy.angularVelocity,
            vehicleBox.transform.position,
            vehicleBox.transform.rotation
            );
    }

    [Rpc(SendTo.NotServer)]
    public void UpdateClientStateRpc(int frameNumber, VehicleState state)
    {

    }

    [Rpc(SendTo.Server)]
    public void UpdateServerInputRpc(int frameNumber, VehicleInput input)
    {
        VehicleTimeStamp stamp = new VehicleTimeStamp();
        stamp.frameNumber = frameNumber;
        stamp.vehicleInput = input;

        vehicleTimeStamp.Add(stamp, frameNumber);
    }

    public void UpdateVehicleInput(VehicleInput input)
    {
        hInput = input.hInput;
        vInput = input.vInput;
    }

    private void FixedUpdate()
    {
        if (IsHost)
        {            
            if(!IsOwner)
            {
                VehicleInput input = vehicleTimeStamp.Get(lastSyncedFrameNumber).vehicleInput;

                hInput = input.hInput;
                vInput = input.vInput;
            }

            StepVehicleMovement();
            UpdateClientStateRpc(lastSyncedFrameNumber, MakeVehicleState());

            lastSyncedFrameNumber++;
        }

        else
        {
            //check for error

            VehicleInput vehicleInput = new(hInput, vInput);
            vehicleTimeStamp.Get(currentFrameNumber).vehicleInput = vehicleInput;
            UpdateServerInputRpc(currentFrameNumber, vehicleInput);

            currentFrameNumber++;

//            Physics.simulationMode = SimulationMode.Script;

//             do
//             {
//                 Physics.Simulate(Time.fixedDeltaTime);
// 
//                 currentFrameNumber++;
//             }
//             while (currentFrameNumber < lastSyncedFrameNumber + clientFrameAheadAmount);
        }




//         UpdateLapPathIndex();
//         distanceFromFirstPlace = SceneManager.GetDistanceFromFirstPlace(this);

//         if (!NetworkManager.IsHost || !SceneManager.Get().raceStarted)
//         {
//             return;
//         }


    }
}
