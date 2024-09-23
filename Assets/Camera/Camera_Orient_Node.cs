using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.UIElements;

public class Camera_Orient_Node : MonoBehaviour
{
    public List<Camera_Orient_Node> Neighbours;

    private void OnTriggerEnter(Collider other)
    {
        SC_TestVehicle vehicle = other.transform.root.GetComponentInChildren<SC_TestVehicle>();
        vehicle.orientNode = this;
    }

    public Vector3 GetCameraUpVector(Vector3 worldPos)
    {
        float min_dist = float.MaxValue;
        Camera_Orient_Node nearest = null;

        foreach (Camera_Orient_Node node in Neighbours)
        {
            float dist = Vector3.Distance(node.transform.position, worldPos);
            if (dist < min_dist)
            {
                min_dist = dist;
                nearest = node;
            }
        }

        Vector3 d = nearest.transform.position - transform.position;

        Vector3 vec1 = d.normalized;
        Vector3 vec2 = worldPos - transform.position;

        float a = Mathf.Clamp01(Vector3.Dot(vec1, vec2) / d.magnitude);

        return Vector3.Lerp(transform.up, nearest.transform.up, a);
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

        EditorUtility.SetDirty(this);
    }

    void Start()
    {

    }


    void Update()
    {

    }
}


[CustomEditor(typeof(Camera_Orient_Node))]
public class Camera_Orient_Node_Editor : Editor
{
    public bool vDown = false;
    public bool bDown = false;

    void OnSceneGUI()
    {
        GameObject obj = target as GameObject;

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
    }

    private void Align()
    {
        Camera_Orient_Node node = target as Camera_Orient_Node;

        Ray ray = new Ray(node.transform.position, Vector3.down);
        RaycastHit hit;
        bool bHit = Physics.Raycast(ray, out hit);

        if(bHit)
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

        Camera_Orient_Node[] nodes = node.transform.root.GetComponentsInChildren<Camera_Orient_Node>();
        foreach(Camera_Orient_Node n in nodes)
        {
            n.OnNeighboursChanged();
        }
    }
}