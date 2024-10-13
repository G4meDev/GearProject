using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum LapPathCheckPoint
{
    checkpoint_1 = 1,
    checkpoint_2 = 2,
    checkpoint_3 = 3,
}

public class LapPath_Node : MonoBehaviour
{
    private LapPath lapPath;

    public List<LapPath_Node> children;
    public List<LapPath_Node> parents;

    public float Dist = -1;

    public GameObject spawnPoint;

    public LapPathCheckPoint checkpoint;
    public bool isStart = false;

    public float nodeIndex = 0;

    private void Start()
    {
        lapPath = transform.root.GetComponent<LapPath>();
    }

    public float GetMaxNodeIndex()
    {
        return lapPath.maxNodeIndex;
    }

    public float GetIndexAtWorldPosition(Vehicle vehicle)
    {
        Vector3 worldPos = vehicle.vehicleProxy.transform.position;

        float minDist = float.MaxValue;
        LapPath_Node minStart = null;
        LapPath_Node minEnd = null;
        float min_t = 0;

        foreach (LapPath_Node node in children)
        {
            Vector3 startToPos = worldPos - transform.position;
            Vector3 startToEnd = node.transform.position - transform.position;

            float dot = Vector3.Dot(startToEnd.normalized, startToPos);
            dot = Mathf.Clamp(dot, 0, startToEnd.magnitude);
            float dist = Vector3.Distance(worldPos, transform.position + startToEnd.normalized * dot);

            if (dist < minDist)
            {
                minDist = dist;

                minStart = this;
                minEnd = node;

                min_t = dot / startToEnd.magnitude;
            }
        }

        foreach (LapPath_Node node in parents)
        {
            Vector3 startToPos = worldPos - node.transform.position;
            Vector3 startToEnd = transform.position - node.transform.position;

            float dot = Vector3.Dot(startToEnd.normalized, startToPos);
            dot = Mathf.Clamp(dot, 0, startToEnd.magnitude);
            float dist = Vector3.Distance(worldPos, node.transform.position + startToEnd.normalized * dot);

            if (dist < minDist)
            {
                minDist = dist;

                minStart = node;
                minEnd = this;

                min_t = dot / startToEnd.magnitude;
            }
        }

        float startIndex = minStart.nodeIndex;
        float endIndex;
        if(minEnd.isStart)
        {
            if(isStart)
            {
                return 0;
            }
            else
            {
                endIndex = startIndex + 1;
            }
        }

        else
        {
            endIndex = minEnd.nodeIndex;
        }

        return Mathf.Lerp(startIndex, endIndex, min_t);
    }

    public float GetDistanceFromStart(Vehicle vehicle)
    {
        float index = GetIndexAtWorldPosition(vehicle);

        int startIndex = (int)Mathf.Floor(index);
        int endIndex = startIndex + 1;

        float fraction = index - startIndex;

        float a = Mathf.Lerp(lapPath.distanceList[startIndex], lapPath.distanceList[endIndex], fraction);

        return a + (vehicle.currentLap-1) * lapPath.GetFullLapDistance();
    }

    Vector3 GetBoxCorner(Vector3 localPos)
    {
        return transform.TransformPoint(localPos / 2);
    }

    private void OnDrawGizmos()
    {
        Vector3 topNorthEast = GetBoxCorner(new(1, 1, 1));
        Vector3 topSouthWest = GetBoxCorner(new(-1, 1, -1));

        Vector3 topCenter = GetBoxCorner(new(0, 1, 0));

        Color color;
        if (isStart)
        {
            color = Color.green;
        }
        else if (checkpoint == LapPathCheckPoint.checkpoint_1)
        {
            color = Color.red;
        }
        else if (checkpoint == LapPathCheckPoint.checkpoint_2)
        {
            color = Color.yellow;
        }
        else
        {
            color = Color.blue;
        }

        DrawHelpers.drawString(nodeIndex.ToString(), topCenter, 0, 0, 24, color);

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

        UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(node);
        UnityEngine.Object obj = PrefabUtility.InstantiatePrefab(prefab);
        GameObject gameObj = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
        
        LapPath_Node newNode = gameObj.GetComponent<LapPath_Node>();
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