using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;

    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;

    public Controller_PID steerPID;

    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
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
            Vector3 d = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
            Vector3 toPos = vehicle.vehicleProxy.transform.position - aiRouteNode_Current.transform.position;

            float dot = Vector3.Dot(d.normalized, toPos);

            // passed target without hitting collision
            if (dot > d.magnitude)
            {
                OnEnterNewRouteNode(aiRouteNode_Target);

                return GetNearestWorldPosition();
            }

            // should check for parent nodes
            else if (dot < 0)
            {
                AI_Route_Node bestParent = null;
                float min_dist = float.MaxValue;

                foreach(AI_Route_Node parent in aiRouteNode_Current.parents)
                {
                    float dist = Vector3.Distance(vehicle.vehicleProxy.transform.position, parent.transform.position);

                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        bestParent = parent;
                    }
                }



                d = aiRouteNode_Current.transform.position - bestParent.transform.position;
                toPos = vehicle.vehicleProxy.transform.position - bestParent.transform.position;

                dot = Vector3.Dot(d.normalized, toPos);
                dot = Mathf.Clamp(dot, 0, d.magnitude);

                return bestParent.transform.position + dot * d.normalized;
            }

            else
            {
                return aiRouteNode_Current.transform.position + dot * d.normalized;
            }
        }

        return Vector3.zero;
    }

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
        steerPID = gameObject.AddComponent<Controller_PID>();
        steerPID.Init(.15f, .035f, 0.1f);
    }

    void Update()
    {
        Vector3 nearestpos = GetNearestWorldPosition();

        float dist = Vector3.Distance(nearestpos, vehicle.vehicleProxy.transform.position);

        Vector3 right = Vector3.Cross(Vector3.up, aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position);

        dist = Vector3.Dot(right, vehicle.vehicleProxy.transform.position - nearestpos) > 0 ? dist : -dist;

        DrawHelpers.DrawSphere(nearestpos, 5, Color.blue);

        float steer = steerPID.Step(0, dist, Time.deltaTime);


        if (vehicle)
        {
            vehicle.vInput = 1;

            vehicle.SetSteerInput(steer);

            //Debug.Log(vehicle.hInput);
        }

    }

}
