using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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


    private static Vector3 HalfRotation = new Vector3(180, 180, 180);

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
            Vector3 OuterPosition = WDI.WheelTransform.position + WDI.WheelWidth * 0.5f * (WDI.bRightWheel ? -WDI.WheelTransform.up : WDI.WheelTransform.up);
            float CylinderHeight = WDI.WheelWidth + WheelsInnerPadding;

            Quaternion quat = WDI.WheelTransform.rotation;
            if (!WDI.bRightWheel)
            {
                Quaternion q = Quaternion.AngleAxis(180, transform.forward);
                quat = q * quat;
            }

            Vector3 dir = quat * Vector3.up;
            Vector3 InnerPosition = OuterPosition + dir * CylinderHeight;

            DrawHelpers.DrawCylinder(OuterPosition, quat, WDI.WheelRadius, CylinderHeight, Color.green);

            Vector3 SweepOffest = WDI.WheelTransform.forward * WDI.WheelRadius * 2;

            RaycastHit[] Results = Physics.CapsuleCastAll(InnerPosition + SweepOffest, OuterPosition + SweepOffest, WDI.WheelRadius, -WDI.WheelTransform.forward, WDI.WheelRadius * 2);
            List<RaycastHit> FilteredResult = new List<RaycastHit>();

            foreach (RaycastHit r in Results)
            {
                DrawHelpers.DrawSphere(r.point, 0.1f, Color.red);

                if (new Vector3(0, 0, 0) == r.point)
                { continue; }

                float Padding = 0.01f;
                Vector3 PaddedOuterPosition = OuterPosition - dir * Padding;
                Plane OuterPlane = new Plane(dir, PaddedOuterPosition);
                float d = OuterPlane.GetDistanceToPoint(r.point);

                if (d < 0 || d > CylinderHeight + Padding * 2)
                { continue; }

                DrawHelpers.DrawSphere(r.point, 0.1f, Color.blue);

                FilteredResult.Add(r);
            }


            float NearestDistance = 10000.0f;
            RaycastHit FinalResult = new RaycastHit();
            bool bFoundHit = false;
            Plane WheelPlane = new Plane(-WDI.WheelTransform.forward, OuterPosition);

            foreach (RaycastHit hit in FilteredResult)
            {
                float distance = WheelPlane.GetDistanceToPoint(hit.point);
                if (distance < NearestDistance)
                {
                    bFoundHit = true;
                    NearestDistance = distance;
                    FinalResult = hit;
                }
            }

            if(bFoundHit)
            {
                DrawHelpers.DrawSphere(FinalResult.point, 0.1f, Color.yellow);
            }



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
