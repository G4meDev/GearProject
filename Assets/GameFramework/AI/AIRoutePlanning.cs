using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIRoutePlanning : MonoBehaviour
{
    private Vehicle vehicle;

    public LinkedList<AI_Route_Node> path = new();

    public RouteData GetRouteData()
    {
        RouteData data = new();

        if (path.Count < 2)
            return data;


        float maxProjectionDistance = AI_Params.projection_3_dist * 1.3f;

        int i = 2;

        while (maxProjectionDistance > 0)
        {
            if (path.Count <= i)
            {
                // @TODO: add route planing
                path.AddLast(path.Last().children[0]);
            }

            float dist = path.ElementAt(i - 1).childDist[0];
            maxProjectionDistance -= dist;

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
            return GetRouteData();
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

        Vector3 inter;

        if (behind)
        {
            inter = Vector3.LerpUnclamped(parentPos, nodePos, dot);
        }
        else
        {
            inter = Vector3.LerpUnclamped(nodePos, childPos, dot);
        }
        
        DrawHelpers.DrawSphere(inter, 3, Color.blue);

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

                float a = Mathf.InverseLerp(currentDist, nextDist, AI_Params.projection_1_dist);
                Vector3 e = Vector3.Lerp(n.Value.transform.position, n.Next.Value.transform.position, a);
                DrawHelpers.DrawSphere(e, 3, Color.yellow);
            }

            if (!projection_2_flag && AI_Params.projection_2_dist > currentDist && AI_Params.projection_2_dist < nextDist)
            {
                projection_2_flag = true;

                float a = Mathf.InverseLerp(currentDist, nextDist, AI_Params.projection_2_dist);
                Vector3 e = Vector3.Lerp(n.Value.transform.position, n.Next.Value.transform.position, a);
                DrawHelpers.DrawSphere(e, 3, Color.yellow);
            }

            if (AI_Params.projection_3_dist > currentDist && AI_Params.projection_3_dist < nextDist)
            {
                float a = Mathf.InverseLerp(currentDist, nextDist, AI_Params.projection_3_dist);
                Vector3 e = Vector3.Lerp(n.Value.transform.position, n.Next.Value.transform.position, a);
                DrawHelpers.DrawSphere(e, 3, Color.yellow);
                break;
            }

            currentDist = nextDist;
            n = n.Next;
        }

        foreach (AI_Route_Node e in path)
        {
            DrawHelpers.DrawSphere(e.transform.position, 5, Color.red);
        }

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

    void Update()
    {
        
    }
}
