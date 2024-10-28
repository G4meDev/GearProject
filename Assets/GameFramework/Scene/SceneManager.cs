using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneManager : NetworkBehaviour
{
    private static SceneManager instance;

    // TODO: player spawning
    public Vehicle playerVehicle;

    public List<AIController> aiControllers = new List<AIController>();

    public List<Vehicle> allVehicles = new List<Vehicle>();


    public GameObject screenInputPrefab;
    public UI_ScreenInput screenInput;

    public GameObject lapCounterPrefab;
    public UI_LapCounter lapCounter;

    public GameObject positionPrefab;
    public UI_Position position;

    public GameObject debugDataPrefab;
    public UI_DebugData debugData;

    public int lapCount = 3;

    public List<GameObject> spawns;

    public bool raceStarted = false;

    public GameObject vehiclePrefab;

    // ----------------------------------------

    public static SceneManager Get()
    {
        if (instance == null)
            instance = GameObject.FindObjectOfType<SceneManager>();
        return instance;
    }

    public void PrepareRace()
    {
        if (IsServer)
        {
            raceStarted = false;

            allVehicles.Clear();

            for (int i = 0; i < NetworkManager.ConnectedClients.Count; i++)
            {
                GameObject vehicle = GameObject.Instantiate(vehiclePrefab, spawns[i].transform.position, spawns[i].transform.rotation);
                NetPlayer netPlayer = NetworkManager.ConnectedClients[(ulong)i].PlayerObject.GetComponent<NetPlayer>();

                netPlayer.vehicle = vehicle.GetComponent<Vehicle>();
                netPlayer.vehicle.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ConnectedClients[(ulong)i].ClientId);

                allVehicles.Add(netPlayer.vehicle);
            }

        }
    }

    public void OnPlayerChanged(Vehicle vehicle)
    {
        if(vehicle.IsServer)
        {
            playerVehicle = vehicle;
            allVehicles.Add(vehicle);
        }

        if (vehicle.IsOwner)
        {
            //screenInput.OnPlayerChanged();
            //lapCounter.UpdateLapCounter();
        }

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
        //return allVehicles[0].distanceFromStart - vehicle.distanceFromStart;

        return 1;
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();


    }


    public void StartRace()
    {


        raceStarted = true;
    }

    void Start()
    {
//#if UNITY_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 60;
//#endif

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
        lapCounter = Instantiate(lapCounterPrefab).GetComponent<UI_LapCounter>();
        position = Instantiate(positionPrefab).GetComponent<UI_Position>();


#if UNITY_EDITOR
        debugData = Instantiate(debugDataPrefab).GetComponent<UI_DebugData>();
#endif


        raceStarted = false;
    }

    void Update()
    {
        UpdateVehiclesPosition();
    }
}
