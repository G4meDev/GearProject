using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class Camera_Orient_Node : MonoBehaviour
{
    public List<Camera_Orient_Node> Neighbours;

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.transform.root.GetComponentInChildren<Vehicle>();
        vehicle.orientNode = this;
    }

    public Vector3 GetCameraUpVector(Vector3 worldPos)
    {
        float min_dist = float.MaxValue;
        Camera_Orient_Node nearest = null;

        Vector3 nearest_pos;

        float dot;
        Vector3 nodeToNearest;
        Vector3 nodeToTarget = worldPos - transform.position;

        float a = 2;

        foreach (Camera_Orient_Node node in Neighbours)
        {
            nearest_pos = node.transform.position;
            nodeToNearest = nearest_pos - transform.position;
            dot = Vector3.Dot(nodeToNearest.normalized, nodeToTarget) / nodeToNearest.magnitude;

            if(dot >= 0 && dot <= 1 && nodeToTarget.magnitude < min_dist)
            {
                a = dot;
                min_dist = nodeToTarget.magnitude;
                nearest = node;
            }
        }

        if (nearest)
        {
            return Vector3.Lerp(transform.up, nearest.transform.up, a);
        }

        return transform.up;
    }

    public void OnNeighboursChanged()
    {
         List<Camera_Orient_Node> seenNodes = new();

        for (int i = Neighbours.Count - 1; i >= 0; i--)
        {
            if (!Neighbours[i] || Neighbours[i] == this || seenNodes.Contains(Neighbours[i]))
            {
                Neighbours.RemoveAt(i);
                continue;
            }

            else
            {
                seenNodes.Add(Neighbours[i]);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Camera_Orient_Node))]
public class Camera_Orient_Node_Editor : Editor
{
    public bool vDown = false;
    public bool bDown = false;

    void OnSceneGUI()
    {
        //GameObject obj = target as GameObject;

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.V && !vDown)
        {
            vDown = true;

            Align();
        }
        
        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.V)
        {
            vDown = false;
        }

        // --------------------------

        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B && !bDown)
        {
            bDown = true;
            Extrude();
        }

        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.B)
        {
            bDown = false;
        }
    }

    private void Align()
    {
        Camera_Orient_Node node = target as Camera_Orient_Node;

        

        Ray ray = new(node.transform.position, Vector3.down);
        bool bHit = Physics.Raycast(ray, out RaycastHit hit);

        if (bHit)
        {
            Vector3 forward = Vector3.Cross(node.transform.right, hit.normal);
            node.transform.rotation = Quaternion.LookRotation(forward, hit.normal);
            EditorUtility.SetDirty(node);
        }
    }

    private void Extrude()
    {
        Camera_Orient_Node node = target as Camera_Orient_Node;

        Object obj = PrefabUtility.InstantiatePrefab(node.GetPrefabDefinition());

        Camera_Orient_Node newNode = obj.GetComponent<Camera_Orient_Node>();
        newNode.transform.parent = node.transform.root;

        newNode.transform.SetPositionAndRotation(node.transform.position + node.transform.forward * 50, node.transform.rotation);
        newNode.transform.localScale = node.transform.localScale;

        int ix = node.name.LastIndexOf('_');

        string s = node.name.Substring(ix + 1, node.name.Length - ix - 1);
        int i = int.Parse(s) + 1;

        s = node.name[..(ix + 1)] + i;
        newNode.name = s;
        
        newNode.Neighbours.Clear();
        newNode.Neighbours.Add(node);

        node.Neighbours.Add(newNode);

        Selection.activeObject = newNode;

        Camera_Orient_Node[] nodes = node.transform.root.GetComponentsInChildren<Camera_Orient_Node>();
        foreach(Camera_Orient_Node n in nodes)
        {
            n.OnNeighboursChanged();
        }
    }
}

#endif