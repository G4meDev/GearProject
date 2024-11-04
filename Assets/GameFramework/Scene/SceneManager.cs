using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

public class SceneManager : NetworkBehaviour
{
    private static SceneManager instance;

    // TODO: player spawning
    public Vehicle localVehicle;

    public List<AIController> aiControllers = new List<AIController>();

    public List<Vehicle> allVehicles = new List<Vehicle>();


    public GameObject screenInputPrefab;
    public UI_ScreenInput screenInput;

    public GameObject lapCounterPrefab;
    public UI_LapCounter lapCounter;

    public GameObject positionPrefab;
    public UI_Position position;

    public CountDown countDown;

    public int lapCount = 3;

    public List<GameObject> spawns;

    public bool raceStarted = false;

    public GameObject vehiclePrefab;

    public bool Desynced = false;

    public uint currentFrame = 0;
    public uint lastSyncedFrame = 0;

    public uint frameAheadTaget = 7;

    // ----------------------------------------

    private void OnGUI()
    {
        ulong rtt = NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
        
        GUIStyle label2 = new GUIStyle(GUI.skin.label);
        label2.fontSize = 72;

        GUI.Label(new Rect(5, Screen.height - 450, 300, 150), rtt.ToString(), label2);
    }

    public static SceneManager Get()
    {
        if (instance == null)
            instance = GameObject.FindObjectOfType<SceneManager>();
        return instance;
    }

    public void PrepareRace()
    {
        if (IsHost)
        {
            raceStarted = false;

            allVehicles.Clear();

            int spawnCounter = 0;

            foreach(NetworkClient client in NetworkManager.ConnectedClientsList)
            {
                NetPlayer netPlayer = client.PlayerObject.GetComponent<NetPlayer>();

                GameObject vehicle = GameObject.Instantiate(vehiclePrefab, spawns[spawnCounter].transform.position, spawns[spawnCounter].transform.rotation);
                netPlayer.vehicle = vehicle.GetComponent<Vehicle>();
                netPlayer.vehicle.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
                
                spawnCounter++;
            }

            countDown.StartCountingRpc();
        }
    }

    public void RegisterVehicle(Vehicle vehicle)
    {
        allVehicles.Add(vehicle);

        if (vehicle.IsOwner)
        {
            OnLocalPlayerChanged(vehicle);
        }
    }

    public void UnregisterVehicle(Vehicle vehicle)
    {
        allVehicles.Remove(vehicle);
    }

    public void OnLocalPlayerChanged(Vehicle vehicle)
    {
        localVehicle = vehicle;
        screenInput.OnPlayerChanged();
        lapCounter.UpdateLapCounter();
    }

    public void RegisterAI(AIController controller)
    {
        aiControllers.Add(controller);
        allVehicles.Add(controller.vehicle);

        controller.UpdateTargetPosition(aiControllers.Count);
    }

    public class VehiclePositionComparer : IComparer<Vehicle>
    {
        public int Compare(Vehicle x, Vehicle y)
        {
            return x.distanceFromStart > y.distanceFromStart ? -1 : 1;
        }
    }

    private void UpdateVehiclesPosition()
    {
        allVehicles.Sort(new VehiclePositionComparer());

        for (int i = 0; i < allVehicles.Count; i++)
        {
            allVehicles[i].position = i + 1;
        }
    }

    public static float GetDistanceFromFirstPlace(Vehicle vehicle)
    {
        return Get().allVehicles[0].distanceFromStart - vehicle.distanceFromStart;
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();


    }


    public void StartRace()
    {
        raceStarted = true;


    }

    public void MarkDesyncAtFrame(uint frame)
    {
        Desynced = true;
        lastSyncedFrame = frame;
    }

    private void Init()
    {
        if (!IsHost)
        {
            currentFrame = frameAheadTaget;
        }

        Application.targetFrameRate = 61;
        //Time.fixedDeltaTime = 1.0f / 30.0f;

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
        lapCounter = Instantiate(lapCounterPrefab).GetComponent<UI_LapCounter>();
        position = Instantiate(positionPrefab).GetComponent<UI_Position>();

        raceStarted = false;
    }

    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        Init();
    }



    void Update()
    {
        if(raceStarted)
        {
            UpdateVehiclesPosition();
        }

    }

    public void ServerUpdate()
    {
        foreach (var vehicle in allVehicles)
        {
            VehicleInput currentInput = vehicle.TryGetRemoteInputForFrame(currentFrame);
            vehicle.UpdateVehicleInput(currentInput);
            vehicle.StepVehicleMovement();
        }
        
        Physics.Simulate(Time.fixedDeltaTime);

        foreach (var vehicle in allVehicles)
        {
            vehicle.UpdateClientStateRpc(currentFrame, vehicle.MakeVehicleState());
        }

        currentFrame++;
    }

    public void RollbackToFrame(uint frame)
    {
        foreach(var vehicle in allVehicles)
        {
            vehicle.UpdateVehicleToState(vehicle.vehicleTimeStamp.Get(frame).vehicleState);
        }
    }

    public void ClientUpdate()
    {
        //TODO: ahead update

        if (Desynced)
        {
            RollbackToFrame(lastSyncedFrame);

            for (uint i = lastSyncedFrame + 1; i < currentFrame; i++)
            {
                foreach (var vehicle in allVehicles)
                {
                    VehicleInput input = vehicle.TryGetRemoteInputForFrame(i);

                    vehicle.UpdateVehicleInput(input);
                    vehicle.StepVehicleMovement();
                }

                Physics.Simulate(Time.fixedDeltaTime);

                foreach (var vehicle in allVehicles)
                {
                    vehicle.vehicleTimeStamp.Get(i).vehicleState = vehicle.MakeVehicleState();
                }
            }
        }

        foreach (var vehicle in allVehicles)
        {
            VehicleInput input = vehicle.TryGetRemoteInputForFrame(currentFrame);

            vehicle.UpdateVehicleInput(input);
            vehicle.StepVehicleMovement();
        }

        Physics.Simulate(Time.fixedDeltaTime);

        foreach (var vehicle in allVehicles)
        {
            vehicle.vehicleTimeStamp.Get(currentFrame).vehicleState = vehicle.MakeVehicleState();
        }

        currentFrame++;
    }

    private void FixedUpdate()
    {
        foreach(var vehicle in allVehicles)
        {
            if (vehicle.IsOwner)
            {
                VehicleInput currentInput = new VehicleInput(vehicle.hInput, vehicle.vInput);
                vehicle.vehicleTimeStamp.Get(currentFrame).vehicleInput = currentInput;
                vehicle.lastRecivedInputFrame = currentFrame;
                vehicle.UpdateInputRpc(currentFrame, currentInput);
            }
        }

        if (IsHost)
        {
            ServerUpdate();
        }

        else
        {
            ClientUpdate();
        }
    }
}
