using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AI_Route_Node : MonoBehaviour
{
    public List<AI_Route_Node> children;
    public List<AI_Route_Node> parents;

    public int GetDriftDirectionToTarget(AI_Route_Node node)
    {
        float a = Vector3.Dot(transform.right, node.transform.forward);
        return Mathf.Abs(a) > AI_Params.maxDriftableTurnAlpha ? (int)Mathf.Sign(a) : 0;
    }

    private void OnDrawGizmos()
    {
        //DrawHelpers.DrawSphere(transform.position + transform.right * transform.lossyScale.x, 2, Color.green);

        foreach (AI_Route_Node child in children)
        {
            if (child)
            {
                Vector3 dir = child.transform.position - transform.position;

                DrawArrow.ForGizmo(transform.position + Vector3.up * 2, transform.forward * dir.magnitude * 0.3f, Color.black, 5);
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
            vehicle.OnEnterNewRouteNode(this);
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

        Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(node);
        Object obj = PrefabUtility.InstantiatePrefab(prefab);
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