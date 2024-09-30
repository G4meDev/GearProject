using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    // TODO: player spawning
    public static Vehicle playerVehicle;

    public static List<AIController> aiControllers = new List<AIController>();


    public GameObject screenInputPrefab;
    private static UI_ScreenInput screenInput;


    public GameObject aeroMeterPrefab;
    private static UI_AeroMeter aeroMeter;


    public GameObject driftMeterPrefab;
    private static UI_DriftMeter driftMeter;

    public GameObject debugDataPrefab;
    private static UI_DebugData debugData;

    // -------------------------------------------------

    public static UI_ScreenInput GetScreenInput() { return screenInput; }

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

    public static float GetOptimalPathChanceForPosition(int pos)
    {
        switch (pos)
        {
            case 1:
                return 0.9f;
            
            case 2:
                return 0.8f;

            case 3:
                return 0.7f;

            case 4:
                return 0.55f;

            case 5:
                return 0.5f;

            case 6:
                return 0.4f;

            case 7:
                return 0.35f;

            default:
                return 0.35f;
        }
    }

    public static void RegisterAI(AIController controller)
    {
        aiControllers.Add(controller);

        controller.optimalPathChance = GetOptimalPathChanceForPosition(aiControllers.Count) - 0.3f;
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
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
