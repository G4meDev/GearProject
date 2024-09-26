using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerInput : MonoBehaviour
{
    Vehicle vehicle;

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    void FixedUpdate()
    {
        UI_ScreenInput screenInput = SceneManager.GetScreenInput();

#if UNITY_EDITOR

        vehicle.vInput = UnityEngine.Input.GetAxis("Vertical");
        vehicle.hInput = UnityEngine.Input.GetAxis("Horizontal");

        vehicle.pressedJump = UnityEngine.Input.GetButtonDown("Jump");
        vehicle.holdingJump = UnityEngine.Input.GetButton("Jump");


        vehicle.hInput = Mathf.Clamp(vehicle.hInput + screenInput.data.hInput, -1, 1);
        vehicle.vInput = Mathf.Clamp(vehicle.vInput + screenInput.data.vInput, -1, 1);
        vehicle.pressedJump |= screenInput.data.pressedJump;
        vehicle.holdingJump |= screenInput.data.holdingJump;

#else

        vehicle.hInput = screenInput.data.hInput;
        vehicle.vInput = screenInput.data.vInput;
        vehicle.pressedJump = screenInput.data.pressedJump;
        vehicle.holdingJump = screenInput.data.holdingJump;
#endif
    }
}
