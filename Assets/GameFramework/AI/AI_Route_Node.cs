using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class RouteData
{
    Vector3 projection_1;
    Vector3 projection_2;
    Vector3 projection_3;
}

public class AI_Route_Node : MonoBehaviour
{
    public List<AI_Route_Node> children;
    public List<AI_Route_Node> parents;
    public List<float> childDist;

//     public Vector3 GetNearestWorldPosition(out float trackWidth, out Vector3 tan)
//     {
//         trackWidth = 20.0f;
//         tan = Vector3.zero;
// 
//         if (aiRouteNode_Current && aiRouteNode_Target)
//         {
//             Vector3 d = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
//             Vector3 toPos = vehicleProxy.transform.position - aiRouteNode_Current.transform.position;
// 
//             float dot = Vector3.Dot(d.normalized, toPos);
// 
//             // passed target without hitting collision
//             if (dot > d.magnitude)
//             {
//                 OnEnterNewRouteNode(aiRouteNode_Target);
// 
//                 return GetNearestWorldPosition(out trackWidth, out tan);
//             }
// 
//             // should check for parent nodes
//             else if (dot < 0)
//             {
//                 AI_Route_Node bestParent = null;
//                 float min_dist = float.MaxValue;
// 
//                 foreach (AI_Route_Node parent in aiRouteNode_Current.parents)
//                 {
//                     float dist = Vector3.Distance(vehicleProxy.transform.position, parent.transform.position);
// 
//                     if (dist < min_dist)
//                     {
//                         min_dist = dist;
//                         bestParent = parent;
//                     }
//                 }
// 
// 
// 
//                 d = aiRouteNode_Current.transform.position - bestParent.transform.position;
//                 toPos = vehicleProxy.transform.position - bestParent.transform.position;
// 
//                 dot = Vector3.Dot(d.normalized, toPos);
//                 dot = Mathf.Clamp(dot, 0, d.magnitude);
// 
//                 float a = dot / d.magnitude;
//                 trackWidth = Mathf.Lerp(bestParent.transform.lossyScale.x, aiRouteNode_Current.transform.lossyScale.x, a);
//                 tan = Vector3.Lerp(bestParent.transform.forward, aiRouteNode_Current.transform.forward, a);
// 
//                 return bestParent.transform.position + dot * d.normalized;
//             }
// 
//             else
//             {
//                 float a = dot / d.magnitude;
// 
//                 trackWidth = Mathf.Lerp(aiRouteNode_Current.transform.lossyScale.x, aiRouteNode_Target.transform.lossyScale.x, a);
//                 tan = Vector3.Lerp(aiRouteNode_Current.transform.forward, aiRouteNode_Target.transform.forward, a);
// 
//                 return aiRouteNode_Current.transform.position + dot * d.normalized;
//             }
//         }
// 
//         return Vector3.zero;
//     }

    public float GetDistToNode(AI_Route_Node inChild)
    {
        int i = children.FindIndex(x => x = inChild);

        return i > 0 ? childDist[i] : 30;
    }

    public int GetDriftDirectionToTarget(AI_Route_Node node)
    {
        float a = Vector3.Dot(transform.right, node.transform.forward);
        return Mathf.Abs(a) > AI_Params.maxDriftableTurnAlpha ? (int)Mathf.Sign(a) : 0;
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < children.Count; ++i)
        {
            var child = children[i];

            if (child)
            {
                Vector3 dir = child.transform.position - transform.position;

                DrawArrow.ForGizmo(transform.position + Vector3.up * 2, transform.forward * dir.magnitude * 0.3f, Color.black, 5);

                float dist = childDist.Count > i ? childDist[i] : 0;
                DrawHelpers.drawString(((int)dist).ToString(), (transform.position + Vector3.up * 3) + dir.magnitude * transform.forward * 0.15f, 0, 0, 34);
            }
        }

        foreach (AI_Route_Node parent in parents)
        {
            if (parent)
            {
                Vector3 offset = Vector3.up * 5;

                Vector3 dir = parent.transform.position - transform.position;
                DrawArrow.ForGizmo(transform.position + offset, (dir.magnitude - 5) * dir.normalized, Color.gray, 5);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.transform.root.GetComponentInChildren<Vehicle>();

        if (vehicle)
        {
            vehicle.routePlanning.OnEnterNewRouteNode(this);
        }
    }

    public void OnNeighboursChanged()
    {

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(AI_Route_Node))]
[CanEditMultipleObjects]
public class AI_Route_Node_Editor : Editor
{
    public bool bDown = false;
    public bool vDown = false;

    void OnSceneGUI()
    {
        //GameObject obj = target as GameObject;

        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B && !bDown)
        {
            bDown = true;
            Extrude();
        }

        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.B)
        {
            bDown = false;
        }

        // -----------------------------------------------------------

        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.V && !bDown)
        {
            vDown = true;
            Align();
        }

        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.V)
        {
            vDown = false;
        }
    }

    private void Extrude()
    {
        AI_Route_Node node = target as AI_Route_Node;

        UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(node);
        UnityEngine.Object obj = PrefabUtility.InstantiatePrefab(prefab);
        GameObject gameObj = PrefabUtility.GetNearestPrefabInstanceRoot(obj);

        AI_Route_Node newNode = gameObj.GetComponent<AI_Route_Node>();
        newNode.transform.parent = node.transform.root;

        newNode.transform.SetPositionAndRotation(node.transform.position + node.transform.forward * 50, node.transform.rotation);
        newNode.transform.localScale = node.transform.localScale;

        int ix = node.name.LastIndexOf('_');

        string s = node.name.Substring(ix + 1, node.name.Length - ix - 1);
        int i = int.Parse(s) + 1;

        s = node.name[..(ix + 1)] + i;
        newNode.name = s;
        
        newNode.children.Clear();
        newNode.parents.Clear();
        newNode.parents.Add(node);

        node.children.Add(newNode);

        Selection.activeObject = newNode;

        AI_Route_Node[] nodes = node.transform.root.GetComponentsInChildren<AI_Route_Node>();
        foreach(AI_Route_Node n in nodes)
        {
            n.OnNeighboursChanged();
        }
        
    }

    private void Align()
    {
        AI_Route_Node node = target as AI_Route_Node;

        Ray ray = new(node.transform.position, -node.transform.up);
        bool bHit = Physics.Raycast(ray, out RaycastHit hit);

        if (bHit)
        {
            Vector3 forward = Vector3.Cross(node.transform.right, hit.normal);
            node.transform.rotation = Quaternion.LookRotation(forward, hit.normal);
            node.transform.position = hit.point;
            EditorUtility.SetDirty(node);
        }
    }
}

#endif