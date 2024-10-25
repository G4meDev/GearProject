using UnityEngine;


public class PlayerInput : MonoBehaviour
{
    Vehicle vehicle;

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    void Update()
    {
        UI_ScreenInput screenInput = SceneManager.GetScreenInput();

#if UNITY_EDITOR
        
        vehicle.holdingJump = UnityEngine.Input.GetButton("Jump");

//         vehicle.hInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Horizontal") + screenInput.data.hInput, -1, 1);
//         vehicle.vInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Vertical") + screenInput.data.vInput, -1, 1);
//         vehicle.holdingJump |= screenInput.data.holdingJump;


        vehicle.hInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Horizontal"), -1, 1);
        vehicle.vInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Vertical"), -1, 1);

#else

        vehicle.hInput = screenInput.data.hInput;
        vehicle.vInput = screenInput.data.vInput;
        vehicle.holdingJump = screenInput.data.holdingJump;
#endif
    }
}
