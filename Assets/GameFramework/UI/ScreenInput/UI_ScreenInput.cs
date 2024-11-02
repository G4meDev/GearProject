using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenInputData
{
    public float hInput = 0.0f;
    public float vInput = 0.0f;

    public bool holdingJump = false;
}

public class UI_ScreenInput : MonoBehaviour
{
    public UI_GearButton steerLeftButton;
    public UI_GearButton steerRightButton;
    public UI_GearButton throttleButton;
    public UI_GearButton reverseButton;
    public UI_GearButton itemButton;

    public Canvas canvas;

    public ScreenInputData data = new();

    public void OnPlayerChanged()
    {
        canvas.worldCamera = SceneManager.Get().localVehicle.GetComponentInChildren<VehicleCamera>().camera;
    }

    public void DeactivateAll()
    {
        throttleButton.OnDeactive();
        reverseButton.OnDeactive();

        steerLeftButton.OnDeactive();
        steerRightButton.OnDeactive();

        itemButton.OnDeactive();
    }

    void Start()
    {
        throttleButton.OnRelease = DeactivateAll;
        reverseButton.OnRelease = DeactivateAll;
        itemButton.OnRelease = DeactivateAll;
    }

    private void Update()
    {
        data.hInput = (steerRightButton.pressed ? 1 : 0) + (steerLeftButton.pressed ? -1 : 0);

        bool steerRightActive = steerRightButton.pressed;
        bool steerLeftActive = steerLeftButton.pressed;

        if (steerRightActive)
        {
            steerRightButton.OnActive();
        }
        else
        {
            steerRightButton.OnDeactive();
        }

        if (steerLeftActive)
        {
            steerLeftButton.OnActive();
        }
        else
        {
            steerLeftButton.OnDeactive();
        }


        bool throttleActive = (throttleButton.pressed | throttleButton.entered) && !reverseButton.entered;
        bool reverseActive = (reverseButton.pressed | reverseButton.entered) && !throttleButton.entered;

        if (throttleActive)
        {
            throttleButton.OnActive();
        }
        else
        {
            throttleButton.OnDeactive();
        }

        if (reverseActive)
        {
            reverseButton.OnActive();
        }
        else
        {
            reverseButton.OnDeactive();
        }



        data.vInput = (throttleActive ? 1 : 0) + (reverseActive ? -1 : 0);
    }
}