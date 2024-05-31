using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.VisualScripting;
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
        public bool bRightWheel;
        public string BoneName;
        public float WheelWidth;
        public float WheelRadius;   
    };

    private Rigidbody RB;
    private Vector3 GravityDirection;

    [SerializeField]
    public WheelDescription[] WheelsDescription;

    [SerializeField]
    public float WheelsInnerPadding = 0.2f;

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
            //DrawHelpers.DrawSphere(WDI.WheelTransform.position, 0.3f, WDI.bRightWheel ? Color.red : Color.blue);

            Vector3 pos1 = new Vector3();
            Vector3 pos2 = new Vector3();

            pos1 = WDI.WheelTransform.position + WDI.WheelWidth * 0.5f * (WDI.bRightWheel ? -WDI.WheelTransform.up : WDI.WheelTransform.up);
            pos2 = WDI.WheelTransform.position + (WDI.WheelWidth * 0.5f + WheelsInnerPadding) * (WDI.bRightWheel ? WDI.WheelTransform.up : -WDI.WheelTransform.up);

            DrawHelpers.DrawCircle(pos1, WDI.WheelTransform.rotation * Quaternion.Euler(0, 0, 0) , 0.2f, Color.green);
            DrawHelpers.DrawBox(pos2, WDI.WheelTransform.rotation * Quaternion.Euler(90, 0, 0), 0.4f, Color.cyan);

            //RaycastHit[] Results = Physics.CapsuleCastAll(,)
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

                Transform P = transform;
                WDI.bRightWheel = Vector3.Dot(P.right, T.position - P.position) > 0;

                WheelsDataInternal.Add(WDI);
            }
            
        }
    }


}
