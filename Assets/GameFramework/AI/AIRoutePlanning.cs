using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RouteProjectionData
{
    public RouteSegmentProjection crossTrackProjection = new();
    public RouteSegmentProjection Projection_1 = new();
    public RouteSegmentProjection Projection_2 = new();
    public RouteSegmentProjection Projection_3 = new();

    public float changedDist = 0;
}

public class RouteSegmentProjection
{
    public AI_Route_Node parent;
    public AI_Route_Node child;
    public float t;
}

public class AIRoutePlanning : MonoBehaviour
{
    private Vehicle vehicle;

    public LinkedList<AI_Route_Node> path = new();
    public RouteProjectionData projectionData = null;


    public RouteProjectionData GetProjectionData()
    {
        RouteProjectionData data = new();

        if (path.Count < 2)
            return null;


        float maxProjectionDistance = AI_Params.projection_3_dist;

        int i = 2;

        while (maxProjectionDistance > 0)
        {
            if (path.Count <= i)
            {
                // @TODO: add route planing
                path.AddLast(path.Last().children[0]);
            }

            float dist = path.ElementAt(i - 1).childDist[0];
            
            // count after target as distance
            if (i != 2)
            {
                maxProjectionDistance -= dist;
            }

            i++;
        }

        LinkedListNode<AI_Route_Node> parent = path.First;
        LinkedListNode<AI_Route_Node> node = parent.Next;
        LinkedListNode<AI_Route_Node> child = node.Next;

        // finding cross track

        Vector3 parentPos = parent.Value.transform.position;
        Vector3 nodePos = node.Value.transform.position;
        Vector3 childPos = child.Value.transform.position;

        Vector3 vehiclePos = vehicle.vehicleProxy.transform.position;

        Vector3 nodeTocChild = childPos - nodePos;
        Vector3 nodeToVehicle = vehiclePos - nodePos;

        float dot = Vector3.Dot(nodeTocChild.normalized, nodeToVehicle);
        dot = dot / nodeTocChild.magnitude;

        bool behind = true;

        // passed target
        if (dot > 1)
        {
            OnEnterNewRouteNode(child.Value);
            return GetProjectionData();
        }

        // vehicle is behind node
        else if (dot < 0)
        {
            behind = true;

            Vector3 parentToNode = nodePos - parentPos;
            Vector3 parentToVehicle = vehiclePos - parentPos;

            dot = Vector3.Dot(parentToNode.normalized, parentToVehicle);
            dot = dot / parentToNode.magnitude;
            dot = Mathf.Clamp01(dot);
        }

        else
        {
            behind = false;
        }

        data.crossTrackProjection.parent = behind ? parent.Value : node.Value;
        data.crossTrackProjection.child = behind ? node.Value : child.Value;
        data.crossTrackProjection.t = dot;

        LinkedListNode<AI_Route_Node> n = behind ? parent : node;
        float distToNext = n.Value.GetDistToNode(n.Next.Value);
        float currentDist = -dot * distToNext;

        bool projection_1_flag = false;
        bool projection_2_flag = false;

        while (n.Next != null)
        {
            distToNext = n.Value.GetDistToNode(n.Next.Value);
            float nextDist = currentDist + distToNext;

            if (!projection_1_flag && AI_Params.projection_1_dist > currentDist && AI_Params.projection_1_dist < nextDist)
            {
                projection_1_flag = true;

                data.Projection_1.parent = n.Value;
                data.Projection_1.child = n.Next.Value;
                data.Projection_1.t = Mathf.InverseLerp(currentDist, nextDist, AI_Params.projection_1_dist);
            }

            if (!projection_2_flag && AI_Params.projection_2_dist > currentDist && AI_Params.projection_2_dist < nextDist)
            {
                projection_2_flag = true;

                data.Projection_2.parent = n.Value;
                data.Projection_2.child = n.Next.Value;
                data.Projection_2.t = Mathf.InverseLerp(currentDist, nextDist, AI_Params.projection_2_dist);
            }

            if (AI_Params.projection_3_dist > currentDist && AI_Params.projection_3_dist < nextDist)
            {
                data.Projection_3.parent = n.Value;
                data.Projection_3.child = n.Next.Value;
                data.Projection_3.t = Mathf.InverseLerp(currentDist, nextDist, AI_Params.projection_3_dist);

                break;
            }

            currentDist = nextDist;
            n = n.Next;
        }

        float d = 0;

        if(projectionData != null)
        {
            if (projectionData.crossTrackProjection.parent == data.crossTrackProjection.parent)
            {
                float d1 = data.crossTrackProjection.t * (data.crossTrackProjection.parent.GetDistToNode(data.crossTrackProjection.child));
                float d2 = projectionData.crossTrackProjection.t * (projectionData.crossTrackProjection.parent.GetDistToNode(projectionData.crossTrackProjection.child));

                d = d1 - d2;
            }

            else if (projectionData.crossTrackProjection.child == data.crossTrackProjection.parent)
            {
                float d1 = data.crossTrackProjection.t * (data.crossTrackProjection.parent.GetDistToNode(data.crossTrackProjection.child));
                float d2 = (1 - projectionData.crossTrackProjection.t) * (projectionData.crossTrackProjection.parent.GetDistToNode(projectionData.crossTrackProjection.child));

                d = d1 + d2;
            }

            else if (projectionData.crossTrackProjection.parent == data.crossTrackProjection.child)
            {
                float d1 = (1 - data.crossTrackProjection.t) * (data.crossTrackProjection.parent.GetDistToNode(data.crossTrackProjection.child));
                float d2 = projectionData.crossTrackProjection.t * (projectionData.crossTrackProjection.parent.GetDistToNode(projectionData.crossTrackProjection.child));

                d = -(d1 + d2);
            }
        }

        data.changedDist = d;


//         foreach (AI_Route_Node e in path)
//         {
//             DrawHelpers.DrawSphere(e.transform.position, , Color.red);
//         }

        return data;
    }

    public void RequestAiRouteExpand()
    {

    }

    private void RecounstructAiRoute(AI_Route_Node node)
    {
        path.Clear();

        //@TODO: choose parents
        path.AddLast(node.parents[0]);

        path.AddLast(node);
    }

    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
        if (path.Count > 2 && path.ElementAt(2) == node)
        {
            path.RemoveFirst();
        }

        else
        {
            RecounstructAiRoute(node);
        }
    }

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
    }

    void FixedUpdate()
    {
        projectionData = GetProjectionData();
    }
}
