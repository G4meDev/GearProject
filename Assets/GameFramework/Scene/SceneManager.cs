using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
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

    public GameObject aeroMeterPrefab;
    private static UI_AeroMeter aeroMeter;


    public GameObject driftMeterPrefab;
    private static UI_DriftMeter driftMeter;

    public GameObject debugDataPrefab;
    private static UI_DebugData debugData;

    public int lap_Count = 3;
    public static int lapCount;

    public bool training_Session = false;
    public static bool trainingSession;

    // -------------------------------------------------

    public static UI_ScreenInput GetScreenInput() { return screenInput; }

    public static UI_LapCounter GetLapCounter() { return lapCounter; }

    public static UI_Position GetPosition() { return position; }

    public static UI_AeroMeter GetAeroMeter() { return aeroMeter; }

    public static UI_DriftMeter GetDriftMeter() { return driftMeter; }

    public static UI_DebugData GetDebugData() { return debugData; }

    // ----------------------------------------


    public static void OnPlayerChanged(Vehicle vehicle)
    {
        playerVehicle = vehicle;
        allVehicles.Add(vehicle);

        aeroMeter.OnPlayerChanged();
        screenInput.OnPlayerChanged();
        driftMeter.OnPlayerChanged();
        lapCounter.UpdateLapCounter();
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
        return allVehicles[0].distanceFromStart - vehicle.distanceFromStart;
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif

        lapCount = lap_Count;
        trainingSession = training_Session;

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
        lapCounter = Instantiate(lapCounterPrefab).GetComponent<UI_LapCounter>();
        position = Instantiate(positionPrefab).GetComponent<UI_Position>();
        aeroMeter = Instantiate(aeroMeterPrefab).GetComponent<UI_AeroMeter>();
        driftMeter = Instantiate(driftMeterPrefab).GetComponent<UI_DriftMeter>();


#if UNITY_EDITOR
        debugData = Instantiate(debugDataPrefab).GetComponent<UI_DebugData>();
#endif
    }

    void Update()
    {
        UpdateVehiclesPosition();
    }
}
