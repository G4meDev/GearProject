using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class VehicleMovementComponent : MonoBehaviour
{
    [System.Serializable]
    public struct WheelDescription
    {
        public string boneName;
        public WheelData wheelData;
    }

    private struct WheelDataInternal
    {
        public string BoneName;
        public float WheelWidth;
        public float WheelRadius;   
    };

    private Rigidbody RB;
    private Vector3 GravityDirection;

    [SerializeField]
    public WheelDescription[] WheelsDescription;

    private List<WheelDataInternal> WheelsDataInternal = new();

    // ------------------------------------------------------------------------------------------------------------

    void Start()
    {
        RB = GetComponent<Rigidbody>();

        GravityDirection = Vector3.down;

        InitWheels();
    }

    void FixedUpdate()
    {
        // Gravity
        Vector3 GravityForce = GravityDirection * Physics.gravity.magnitude * Time.fixedDeltaTime;
        RB.AddForce(GravityForce.x, GravityForce.y, GravityForce.z, ForceMode.VelocityChange);

        
        foreach (WheelDataInternal WDI in WheelsDataInternal)
        {
            Transform t = transform.Find("VehicleMesh/root/" + WDI.BoneName);
            if (t)
            {
                Debug.Log("logging");
                DrawHelpers.DrawSphere(t.position, 0.3f, Color.red);
            }
            else
            {
                Debug.LogError(name + ": no bone with name _ " + WDI.BoneName);
            }
        }
    }

    void InitWheels()
    {
        foreach (WheelDescription WheelDesc in WheelsDescription)
        {
            
            if (WheelDesc.wheelData)
            {
                WheelDataInternal WDI = new WheelDataInternal();
                WDI.BoneName = WheelDesc.boneName;
                WDI.WheelWidth = WheelDesc.wheelData.WheelWidth;
                WDI.WheelRadius = WheelDesc.wheelData.WheelRadius;

                WheelsDataInternal.Add(WDI);
            }
            else 
            {
                Debug.LogError(name + ": missing WheelData!");
            }
        }
    }


}
