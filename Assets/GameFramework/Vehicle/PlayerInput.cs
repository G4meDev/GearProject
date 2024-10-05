using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerInput : MonoBehaviour
{
    Vehicle vehicle;

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    public float hInput;
    public float vInput;

    public bool holdingJump;

    void Update()
    {
        UI_ScreenInput screenInput = SceneManager.GetScreenInput();

#if UNITY_EDITOR
        
        holdingJump = UnityEngine.Input.GetButton("Jump");
        
        hInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Horizontal") + screenInput.data.hInput, -1, 1);
        vInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Vertical") + screenInput.data.vInput, -1, 1);
        holdingJump |= screenInput.data.holdingJump;

#else

        hInput = screenInput.data.hInput;
        vInput = screenInput.data.vInput;
        pressedJump = screenInput.data.pressedJump;
        holdingJump = screenInput.data.holdingJump;
#endif
    }
}
