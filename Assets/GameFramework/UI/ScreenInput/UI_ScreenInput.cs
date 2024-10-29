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
    public UI_SteerButton steerButton;
    public UI_GearButton throttleButton;
    public UI_GearButton reverseButton;
    public UI_GearButton jumpButton;
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
        jumpButton.OnDeactive();
    }

    void Start()
    {
        throttleButton.OnRelease = DeactivateAll;
        reverseButton.OnRelease = DeactivateAll;
        jumpButton.OnRelease = DeactivateAll;
        itemButton.OnRelease = DeactivateAll;
    }

    private void Update()
    {
        data.hInput = steerButton.steerValue;

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

        data.holdingJump = (jumpButton.pressed | jumpButton.entered) && !throttleButton.entered && !reverseButton.entered;

        if (data.holdingJump)
        {
            jumpButton.OnActive();
        }
        else
        {
            jumpButton.OnDeactive();
        }
    }
}