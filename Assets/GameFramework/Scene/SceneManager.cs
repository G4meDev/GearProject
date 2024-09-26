using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    // TODO: player spawning
    public static Vehicle playerVehicle;


    public GameObject screenInputPrefab;
    private static UI_ScreenInput screenInput;


    public GameObject aeroMeterPrefab;
    private static UI_AeroMeter aeroMeter;


    public GameObject driftMeterPrefab;
    private static UI_DriftMeter driftMeter;

    // -------------------------------------------------

    public static UI_ScreenInput GetScreenInput() { return screenInput; }

    public static UI_AeroMeter GetAeroMeter() { return aeroMeter; }

    public static UI_DriftMeter GetDriftMeter() { return driftMeter; }

    // ----------------------------------------


    public static void OnPlayerChanged(Vehicle vehicle)
    {
        playerVehicle = vehicle;

        aeroMeter.OnPlayerChanged();
        screenInput.OnPlayerChanged();
        driftMeter.OnPlayerChanged();
    }


    void Start()
    {
        Application.targetFrameRate = 60;

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
        aeroMeter = Instantiate(aeroMeterPrefab).GetComponent<UI_AeroMeter>();
        driftMeter = Instantiate(driftMeterPrefab).GetComponent<UI_DriftMeter>();
    }

    void Update()
    {
        
    }
}
