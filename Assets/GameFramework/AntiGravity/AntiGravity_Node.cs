using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AntiGravity_Node : MonoBehaviour
{
    public List<AntiGravity_Node> Neighbours;

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.transform.root.GetComponentInChildren<Vehicle>();
        vehicle.antiGravityNode = this;
    }

    private void OnTriggerExit(Collider other)
    {
        // is it start or end
        if(Neighbours.Count == 1)
        {
            Vehicle vehicle = other.transform.root.GetComponentInChildren<Vehicle>();

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
    }

    public Vector3 GetUpVector(Vector3 worldPos)
    {
        float min_dist = float.MaxValue;
        AntiGravity_Node nearest = null;

        Vector3 nearest_pos;

        float dot;
        Vector3 nodeToNearest;
        Vector3 nodeToTarget = worldPos - transform.position;

        float a = 2;

        foreach (AntiGravity_Node node in Neighbours)
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
         List<AntiGravity_Node> seenNodes = new();

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

    void Start()
    {

    }


    void Update()
    {

    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(AntiGravity_Node))]
public class AntiGravity_Node_Editor : Editor
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
        AntiGravity_Node node = target as AntiGravity_Node;

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
        AntiGravity_Node node = target as AntiGravity_Node;

        Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(node);
        Object obj = PrefabUtility.InstantiatePrefab(prefab);
        GameObject gameObj = PrefabUtility.GetNearestPrefabInstanceRoot(obj);

        AntiGravity_Node newNode = gameObj.GetComponent<AntiGravity_Node>();
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

        AntiGravity_Node[] nodes = node.transform.root.GetComponentsInChildren<AntiGravity_Node>();
        foreach(AntiGravity_Node n in nodes)
        {
            n.OnNeighboursChanged();
        }

    }
}

#endif