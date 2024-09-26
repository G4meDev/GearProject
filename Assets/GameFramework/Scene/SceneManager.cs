using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject screenInputPrefab;
    private static UI_ScreenInput screenInput;

    // TODO: player spawning
    public static Vehicle playerVehicle;

    public GameObject aeroMeterPrefab;
    private static UI_AeroMeter aeroMeter;

    // -------------------------------------------------

    public static UI_ScreenInput GetScreenInput() { return screenInput; }

    public static UI_AeroMeter GetAeroMeter() { return aeroMeter; }

    // ----------------------------------------


    public static void OnPlayerChanged(Vehicle vehicle)
    {
        playerVehicle = vehicle;

        aeroMeter.OnPlayerChanged();
        screenInput.OnPlayerChanged();
    }


    void Start()
    {
        Application.targetFrameRate = 60;

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
        aeroMeter = Instantiate(aeroMeterPrefab).GetComponent<UI_AeroMeter>();
    }

    void Update()
    {
        
    }
}
