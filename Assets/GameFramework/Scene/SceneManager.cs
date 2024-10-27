using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneManager : NetworkBehaviour
{
    public NetworkManager networkManager;

    // TODO: player spawning
    public static Vehicle playerVehicle;

    public static List<AIController> aiControllers = new List<AIController>();

    public static List<Vehicle> allVehicles = new List<Vehicle>();


    public GameObject screenInputPrefab;
    private static UI_ScreenInput screenInput;

    public GameObject lapCounterPrefab;
    private static UI_LapCounter lapCounter;

    public GameObject positionPrefab;
    private static UI_Position position;

    public GameObject debugDataPrefab;
    private static UI_DebugData debugData;

    public int lap_Count = 3;
    public static int lapCount;

    public bool training_Session = false;
    public static bool trainingSession;

    public List<GameObject> spawns;

    public static bool raceStarted = false;

    // -------------------------------------------------

    public static UI_ScreenInput GetScreenInput() { return screenInput; }

    public static UI_LapCounter GetLapCounter() { return lapCounter; }

    public static UI_Position GetPosition() { return position; }

    public static UI_DebugData GetDebugData() { return debugData; }

    // ----------------------------------------


    public static void OnPlayerChanged(Vehicle vehicle)
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

    public static void RegisterAI(AIController controller)
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
        networkManager = GetComponent<NetworkManager>();


//#if UNITY_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 60;
//#endif

        lapCount = lap_Count;
        trainingSession = training_Session;

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
