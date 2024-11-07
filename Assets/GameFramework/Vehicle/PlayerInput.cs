using Unity.Netcode;
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
        if (!vehicle.IsOwner)
        {
            return;
        }

        UI_ScreenInput screenInput = SceneManager.Get().screenInput;

#if UNITY_EDITOR
        
        float steerInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Horizontal") + screenInput.data.hInput, -1, 1);
        float throttleInput = Mathf.Clamp(UnityEngine.Input.GetAxis("Vertical") + screenInput.data.vInput, -1, 1);

        vehicle.currentHInput = steerInput;
        vehicle.currentVInput = throttleInput;

#else

        vehicle.currentHInput = screenInput.data.hInput;
        vehicle.currentVInput = screenInput.data.vInput;
#endif
    }
}
