using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    // TODO: player spawning
    public static Vehicle playerVehicle;

    public static List<AIController> aiControllers = new List<AIController>();


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

        aeroMeter.OnPlayerChanged();
        screenInput.OnPlayerChanged();
        driftMeter.OnPlayerChanged();
    }

    public static void RegisterAI(AIController controller)
    {
        aiControllers.Add(controller);

        controller.UpdateTargetPosition(aiControllers.Count);
    }

    void Start()
    {
        Application.targetFrameRate = 60;

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
        
    }
}
