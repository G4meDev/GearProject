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
        public Transform WheelTransform;
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
            DrawHelpers.DrawSphere(WDI.WheelTransform.position, 0.3f, Color.red);
           
        }
    }

    void InitWheels()
    {
        foreach (WheelDescription WheelDesc in WheelsDescription)
        {
            if (WheelDesc.wheelData == null)
            {
                Debug.LogError(name + ": missing WheelData!");
            }    

            Transform T = transform.Find("VehicleMesh/root/" + WheelDesc.boneName);
            if (T == null)
            {
                Debug.LogError(name + ": no bone with name _ " + WheelDesc.boneName);
            }

            else
            {
                WheelDataInternal WDI = new WheelDataInternal();
                WDI.BoneName = WheelDesc.boneName;
                WDI.WheelWidth = WheelDesc.wheelData.WheelWidth;
                WDI.WheelRadius = WheelDesc.wheelData.WheelRadius;
                WDI.WheelTransform = T;

                WheelsDataInternal.Add(WDI);
            }
            
        }
    }


}
