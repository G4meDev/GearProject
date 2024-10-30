using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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

    // ----------------------------------------

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
                
                allVehicles.Add(netPlayer.vehicle);

                spawnCounter++;
            }

            countDown.StartCountingRpc();
        }
    }

    public void OnPlayerChanged(Vehicle vehicle)
    {
        if(vehicle.IsOwner)
        {
            localVehicle = vehicle;
            screenInput.OnPlayerChanged();
            lapCounter.UpdateLapCounter();
        }

        if (vehicle.IsServer)
        {

            allVehicles.Add(vehicle);
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

        raceStarted = false;
    }

    void Update()
    {
        UpdateVehiclesPosition();
    }
}
