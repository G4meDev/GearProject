using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;

    public AI_Route_Node aiRouteNode_Previous;
    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;

    public float projectionDistance = 3.0f;

    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
        aiRouteNode_Previous = aiRouteNode_Current;
        aiRouteNode_Current = node;

        UpdateTargetNode();
    }

    public void UpdateTargetNode()
    {
        if (aiRouteNode_Current.children.Count > 0)
        {
            aiRouteNode_Target = aiRouteNode_Current.children[0];
        }

        else
        {
            aiRouteNode_Current = null;
        }
    }

    public Vector3 GetNearestWorldPosition()
    {
        if (aiRouteNode_Current && aiRouteNode_Target)
        {
            Vector3 d_n = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
            Vector3 toPos_n = vehicle.vehicleProxy.transform.position - aiRouteNode_Current.transform.position;

            float dot_n = Vector3.Dot(d_n.normalized, toPos_n);

            Debug.Log(dot_n);

            // we passed target without hitting collision
            if (dot_n > d_n.magnitude)
            {
                OnEnterNewRouteNode(aiRouteNode_Target);

                return GetNearestWorldPosition();
            }

            else if (dot_n < 0 && aiRouteNode_Previous)
            {
                Vector3 d_p = aiRouteNode_Current.transform.position - aiRouteNode_Previous.transform.position;
                Vector3 toPos_p = vehicle.vehicleProxy.transform.position - aiRouteNode_Previous.transform.position;

                float dot_p = Vector3.Dot(d_p.normalized, toPos_p);
                dot_p += projectionDistance;

                float remaining = d_p.magnitude - dot_p;
                if (remaining > 0)
                {
                    return aiRouteNode_Previous.transform.position + dot_p * d_p.normalized;
                }
                else
                {
                    d_p = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
                    return aiRouteNode_Current.transform.position - remaining * d_p.normalized;
                }
            }

            else
            {
                return aiRouteNode_Current.transform.position + (dot_n + projectionDistance) * d_n.normalized;
            }
        }

        return Vector3.zero;
    }

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    void Update()
    {
        if (vehicle)
        {
            //vehicle.vInput = 1;
        }

        DrawHelpers.DrawSphere(GetNearestWorldPosition(), 5, Color.blue);
    }

}
