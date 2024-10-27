using UnityEngine;
using UnityEngine.UI;

public class UI_DebugData : MonoBehaviour
{
    public Text speedText;
    public Text boostText;
    public Text reserveText;

    void Start()
    {
        
    }

    void Update()
    {
        if(SceneManager.Get().playerVehicle)
        {
            speedText.text = string.Format("Speed : {0:F2}", SceneManager.Get().playerVehicle.forwardSpeed);

            boostText.text = string.Format("Boost : {0:F2}", SceneManager.Get().playerVehicle.speedModifierIntensity);
            reserveText.text = string.Format("Reserve : {0:F2}", SceneManager.Get() .playerVehicle.speedModifierReserveTime);
        }
    }
}
