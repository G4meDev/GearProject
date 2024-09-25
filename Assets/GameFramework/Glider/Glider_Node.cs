using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class Glider_Node : MonoBehaviour
{
    public Glider_Node next;

    private void OnTriggerEnter(Collider other)
    {
        //SC_TestVehicle vehicle = other.transform.root.GetComponentInChildren<SC_TestVehicle>();
        //vehicle.antiGravityNode = this;
    }

    private void OnTriggerExit(Collider other)
    {
        // is it start or end

        /*
        
        if(Neighbours.Count == 1)
        {
            SC_TestVehicle vehicle = other.transform.root.GetComponentInChildren<SC_TestVehicle>();

            if(vehicle)
            {
                Vector3 triggerExitDir = Vector3.Dot(transform.forward, Neighbours[0].transform.position - transform.position) > 0
                    ? -transform.forward : transform.forward;
                
                // we exit from back
                if(Vector3.Dot(triggerExitDir, vehicle.vehicleProxy.position - transform.position) > 0)
                {
                    vehicle.antiGravityNode = null;
                }

            }
        }
        */
    }

    public Vector3 GetUpVector(Vector3 worldPos)
    {
        /*
        float min_dist = float.MaxValue;
        Glider_Node nearest = null;

        Vector3 nearest_pos;

        float dot;
        Vector3 nodeToNearest;
        Vector3 nodeToTarget = worldPos - transform.position;

        float a = 2;

        foreach (Glider_Node node in Neighbours)
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

        */

        return transform.up;
    }

    public void OnNeighboursChanged()
    {
        /*
         List<Glider_Node> seenNodes = new();

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
        */
    }

    void Start()
    {

    }


    void Update()
    {

    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Glider_Node))]
public class Glider_Node_Editor : Editor
{
    public bool vDown = false;
    public bool bDown = false;

    void OnSceneGUI()
    {
        //GameObject obj = target as GameObject;

        /*
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

        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Z)
        {
            bDown = false;
        }
        */
    }

    private void Align()
    {
        Glider_Node node = target as Glider_Node;

        Ray ray = new(node.transform.position, -node.transform.up);
        bool bHit = Physics.Raycast(ray, out RaycastHit hit);

        if(bHit)
        {
            Vector3 forward = Vector3.Cross(node.transform.right, hit.normal);
            node.transform.rotation = Quaternion.LookRotation(forward, hit.normal);
            EditorUtility.SetDirty(node);
        }
    }

    private void Extrude()
    {
        /*
        Glider_Node node = target as Glider_Node;

        Object obj = PrefabUtility.InstantiatePrefab(node.GetPrefabDefinition());

        Glider_Node newNode = obj.GetComponent<Glider_Node>();
        newNode.transform.parent = node.transform.root;

        newNode.transform.position = node.transform.position + node.transform.forward * 50;
        newNode.transform.rotation = node.transform.rotation;
        newNode.transform.localScale = node.transform.localScale;

        int ix = node.name.LastIndexOf('_');

        string s = node.name.Substring(ix + 1, node.name.Length - ix - 1);
        int i = int.Parse(s) + 1;

        s = node.name.Substring(0, ix + 1) + i;
        newNode.name = s;
        
        newNode.Neighbours.Clear();
        newNode.Neighbours.Add(node);

        node.Neighbours.Add(newNode);

        Selection.activeObject = newNode;

        Glider_Node[] nodes = node.transform.root.GetComponentsInChildren<Glider_Node>();
        foreach(Glider_Node n in nodes)
        {
            n.OnNeighboursChanged();
        }
        */
    }
}

#endif