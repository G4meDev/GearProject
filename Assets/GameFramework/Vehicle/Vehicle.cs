using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
    public Vector3 vehicleProxy_Position;
    public Quaternion vehicleProxy_Rotation;
    public Vector3 vehicleProxy_Velocity;
    public Vector3 vehicleProxy_AngularVelocity;

    public VehicleState(Vector3 proxy_pos, Quaternion proxy_rot, Vector3 proxy_velo, Vector3 proxy_angularVelo)
    {
        vehicleProxy_Position = proxy_pos;
        vehicleProxy_Rotation = proxy_rot;
        vehicleProxy_Velocity = proxy_velo;
        vehicleProxy_AngularVelocity = proxy_angularVelo;
    }

    public VehicleState() : this(Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero)
    {

    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref vehicleProxy_Position);
        serializer.SerializeValue(ref vehicleProxy_Rotation);
        serializer.SerializeValue(ref vehicleProxy_Velocity);
        serializer.SerializeValue(ref vehicleProxy_AngularVelocity);
    }
}

public class VehicleInput : INetworkSerializable
{
    public float hInput;
    public float vInput;
 
    public VehicleInput(float h, float v)
    {
        hInput = h;
        vInput = v;
    }

    public VehicleInput() : this(0, 0)
    {

    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hInput);
        serializer.SerializeValue(ref vInput);
    }
}

public class VehicleTimeStamp
{
    public VehicleInput vehicleInput;
    public VehicleState vehicleState;

    public VehicleTimeStamp() : this(new VehicleInput(), new VehicleState())
    {

    }

    public VehicleTimeStamp(VehicleInput input, VehicleState state)
    {
        vehicleInput = input;
        vehicleState = state;
    }
}

public class Vehicle : NetworkBehaviour
{
    public VehicleWheel[] wheels;

    public CircularBuffer<VehicleTimeStamp> vehicleTimeStamp = new(128);

    public GameObject serverRepObject;

    public bool isPlayer = true;

    public Rigidbody vehicleProxy;
    public GameObject vehicleMesh;

    public int position = 1;

    public float gravityStr = 25.0f;
    private Vector3 gravityDir = Vector3.down;

    public AnimationCurve torqueCurve;
    [HideInInspector]
    public float avaliableTorque;


    public float maxSpeed = 20.0f;
    public float maxSpeedReverse = -10.0f;
    public float accel = 10.0f;

    [HideInInspector]
    public float speedRatio;



    public float rayDist = 1.0f;

    public float speedModifierIntensity;
    public float speedModifierReserveTime;

    public AnimationCurve steerCurve;
    [HideInInspector]
    public float currentSteer;

    [HideInInspector]
    public float forwardSpeed = 0.0f;


    public float hInput;
    public float vInput;

    public float currentHInput;
    public float currentVInput;

    public float hInputRaiseRate = 0.004f;
    public float vInputRaiseRate = 0.06f;


    public AntiGravity_Node antiGravityNode;

    public LapPath_Node lapPathNode;
    public float distanceFromStart = -1;
    public int currentLap = 1;

    public float distanceFromFirstPlace = -1;

    public delegate void KillDelegate();
    public KillDelegate killDelegate;

    NetPlayer GetOwnerNetPlayer()
    {
        return NetworkManager.ConnectedClients.GetValueOrDefault(OwnerClientId).PlayerObject.GetComponent<NetPlayer>();
    }

    public void EndRace()
    {
        Debug.Log(name + "    End Race!");

        if(IsServer)
        {
            //GetOwnerNetPlayer().OnEndedRaceRpc(position, vehicleBox.position, vehicleBox.rotation);
            NetworkObject.Despawn();
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

//         Ray ray = new(lapPathNode.spawnPoint.transform.position + Vector3.up * 2, Vector3.down);
//         LayerMask layerMask = LayerMask.GetMask("Default");
//         bool bhit = Physics.Raycast(ray, out hit, 5, layerMask);
// 
//         Vector3 targetPos = bhit ? hit.point + Vector3.up * 0.65f : lapPathNode.spawnPoint.transform.position;

//         vehicleProxy.position = targetPos;
//         vehicleMesh.transform.SetPositionAndRotation(targetPos, lapPathNode.spawnPoint.transform.rotation);

        killDelegate?.Invoke();
    }

    public void StartFalling()
    {

    }

//     public float GetMaxSpeedWithModifiers()
//     {
//         float modifier = speedModifierIntensity;
// 
//         return maxSpeed + modifier - GetContactSurfaceFriction() - (Mathf.Abs(steerValue) * steerVelocityFriction);
//     }

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

    private void Gravity()
    {
        if (antiGravityNode)
        {
            gravityDir = -antiGravityNode.GetUpVector(vehicleProxy.position);
        }

        else
        {
            gravityDir = Vector3.down;
        }

        // gravity force
        vehicleProxy.AddForce(gravityDir * gravityStr, ForceMode.Acceleration);
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
            gameObject.AddComponent<PlayerInput>();
        }
    }

    public override void OnNetworkSpawn()
    {
        Init();

        SceneManager.Get().RegisterVehicle(this);

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.Get().UnregisterVehicle(this);

        base.OnNetworkDespawn();
    }

    void Start()
    {

    }

    private void Update()
    {

    }
    
    public void StepVehicleMovement()
    {
        if(currentHInput == 0 && hInput != 0)
        {
            float fallDir = -Mathf.Sign(hInput);
            hInput += fallDir * hInputRaiseRate;
            hInput = Mathf.Sign(hInput) == Mathf.Sign(fallDir) ? 0.0f : hInput;
        }

        else if(currentHInput != 0)
        {
            float raiseDir = Mathf.Sign(currentHInput);
            hInput += raiseDir * hInputRaiseRate;
            hInput = Mathf.Clamp(hInput, -1, 1);
        }


        if (currentVInput == 0 && vInput != 0)
        {
            float fallDir = -Mathf.Sign(vInput);
            vInput += fallDir * vInputRaiseRate;
            vInput = Mathf.Sign(vInput) == Mathf.Sign(fallDir) ? 0.0f : vInput;
        }

        else if (currentVInput != 0)
        {
            float raiseDir = Mathf.Sign(currentVInput);
            vInput += raiseDir * vInputRaiseRate;
            vInput = Mathf.Clamp(vInput, -1, 1);
        }


        forwardSpeed = Vector3.Dot(vehicleProxy.velocity, vehicleProxy.transform.forward);

        speedRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
        avaliableTorque = torqueCurve.Evaluate(speedRatio) * accel * vInput;

        currentSteer = steerCurve.Evaluate(speedRatio) * hInput;

        Gravity();

        foreach(VehicleWheel wheel in wheels)
        {
            wheel.StepPhysic();
        }

        //Physics.SyncTransforms();
    }

    public VehicleState MakeVehicleState()
    {
        return new VehicleState(
            vehicleProxy.position,
            vehicleProxy.rotation,
            vehicleProxy.velocity,
            vehicleProxy.angularVelocity
            );
    }

    public void UpdateVehicleToState(VehicleState state)
    {
        vehicleProxy.position = state.vehicleProxy_Position;
        vehicleProxy.rotation = state.vehicleProxy_Rotation;
        vehicleProxy.velocity = state.vehicleProxy_Velocity;
        vehicleProxy.angularVelocity = state.vehicleProxy_AngularVelocity;

        Physics.SyncTransforms();
    }

    private float pos_error_treshold = 0.5f;
    private float rot_error_treshold = 30.0f;

    public uint lastRecivedInputFrame = 0;

    public bool StatesInSync(VehicleState state1, VehicleState state2)
    {
        float position_Error = Vector3.Distance(state1.vehicleProxy_Position, state2.vehicleProxy_Position);
        float rotation_Error = Quaternion.Angle(state1.vehicleProxy_Rotation, state2.vehicleProxy_Rotation);

        DrawHelpers.DrawSphere(state1.vehicleProxy_Position, 1, Color.red);
        DrawHelpers.DrawSphere(state2.vehicleProxy_Position, 1, Color.blue);

        if ((position_Error > pos_error_treshold) || (rotation_Error > rot_error_treshold))
        {
            Debug.Log("client_" + OwnerClientId + " desynced with " + position_Error + " position and " + rotation_Error + " rotation error");

            return false;
        }

        return true;
    }

    [Rpc(SendTo.NotServer)]
    public void UpdateClientStateRpc(uint frameNumber, VehicleState state)
    {
        serverRepObject.transform.position = state.vehicleProxy_Position;
        serverRepObject.transform.rotation = state.vehicleProxy_Rotation;

        bool Synced = StatesInSync(state, vehicleTimeStamp.Get(frameNumber).vehicleState);

        if(!Synced)
        {
            SceneManager.Get().MarkDesyncAtFrame(frameNumber);

            vehicleTimeStamp.Get(frameNumber).vehicleState = state;
        }
    }

    [Rpc(SendTo.NotOwner)]
    public void UpdateInputRpc(uint frameNumber, VehicleInput input)
    {
        vehicleTimeStamp.Get(frameNumber).vehicleInput = input;
        lastRecivedInputFrame = frameNumber;
    }

    public void UpdateVehicleInput(VehicleInput input)
    {
        hInput = input.hInput;
        vInput = input.vInput;
    }

    public VehicleInput TryGetRemoteInputForFrame(uint frame)
    {
        if (frame <= lastRecivedInputFrame)
        {
            return vehicleTimeStamp.Get(frame).vehicleInput;
        }

        if(IsServer)
            Debug.Log("cient_" + OwnerClientId + " is starving input. last received input on frame " + lastRecivedInputFrame + " requested input for frame " + frame);

        return vehicleTimeStamp.Get(lastRecivedInputFrame).vehicleInput;
    }

    private void FixedUpdate()
    {
        if(IsOwnedByServer && Input.GetKey(KeyCode.E))
        {
            EndRace();
        }

         UpdateLapPathIndex();
         distanceFromFirstPlace = SceneManager.GetDistanceFromFirstPlace(this);

         if (!NetworkManager.IsHost || !SceneManager.Get().raceStarted)
         {
             return;
         }


    }
}
