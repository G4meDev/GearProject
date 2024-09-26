using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject screenInputPrefab;
    private static UI_ScreenInput screenInput;

    // TODO: player spawning
    public static Vehicle playerVehicle;

    public static UI_ScreenInput GetScreenInput() { return screenInput; }

    void Start()
    {
        Application.targetFrameRate = 60;

        screenInput = Instantiate(screenInputPrefab).GetComponent<UI_ScreenInput>();
    }

    void Update()
    {
        
    }
}
