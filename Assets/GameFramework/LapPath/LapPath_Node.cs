using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class LapPath_Node : MonoBehaviour
{
    public List<LapPath_Node> children;
    public List<LapPath_Node> parents;

    public GameObject spawnPoint;

    public float nodeIndex = 0;

    Vector3 GetBoxCorner(Vector3 localPos)
    {
        return transform.TransformPoint(localPos / 2);
    }

    private void OnDrawGizmos()
    {
        Vector3 topNorthEast = GetBoxCorner(new(1, 1, 1));
        Vector3 topSouthWest = GetBoxCorner(new(-1, 1, -1));

        Vector3 topCenter = GetBoxCorner(new(0, 1, 0));

        if (spawnPoint)
        {
            Vector3 dir = spawnPoint.transform.position - topCenter;
            DrawArrow.ForGizmo(topCenter, dir, Color.yellow, 5);
        }

        foreach (LapPath_Node child in children)
        {
            if (child)
            {
                Vector3 childTopSouthEast = child.GetBoxCorner(new(1, 1, -1));
            
                Vector3 dir = childTopSouthEast - topNorthEast;
                DrawArrow.ForGizmo(topNorthEast, dir, Color.red, 5);
            }
        }

        foreach (LapPath_Node parent in parents)
        {
            if (parent)
            {
                Vector3 parentTopNorthWest = parent.GetBoxCorner(new(-1, 1, 1));

                Vector3 dir = parentTopNorthWest - topSouthWest;
                DrawArrow.ForGizmo(topSouthWest, dir, Color.blue, 5);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.transform.root.GetComponentInChildren<Vehicle>();

        if (vehicle)
        {
            vehicle.OnEnterLapPath(this);
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

[CustomEditor(typeof(LapPath_Node))]
[CanEditMultipleObjects]
public class LapPath_Node_Editor : Editor
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
        LapPath_Node node = target as LapPath_Node;

        Object obj = PrefabUtility.InstantiatePrefab(node.GetPrefabDefinition());

        LapPath_Node newNode = obj.GetComponent<LapPath_Node>();
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

        LapPath_Node[] nodes = node.transform.root.GetComponentsInChildren<LapPath_Node>();
        foreach(LapPath_Node n in nodes)
        {
            n.OnNeighboursChanged();
        }
        
    }

    private void Align()
    {
        LapPath_Node node = target as LapPath_Node;

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