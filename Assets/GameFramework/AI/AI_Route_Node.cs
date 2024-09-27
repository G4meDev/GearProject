using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class AI_Route_Node : MonoBehaviour
{
    public List<AI_Route_Node> children;

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.transform.root.GetComponentInChildren<Vehicle>();

        AIController controller = vehicle.GetComponent<AIController>();
        if (controller)
        {
            controller.OnEnterNewRouteNode(this);
        }
    }

//     public Vector3 GetDesigeredVelocity(Vector3 worldPos)
//     {
//         //return Vector3.Normalize(next.transform.position - transform.position) * strength;
//         return Vector3.Normalize(next.transform.position - worldPos) * strength;
//     }

    //public Vector3 Get

    public void OnNeighboursChanged()
    {

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(AI_Route_Node))]
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

        Object obj = PrefabUtility.InstantiatePrefab(node.GetPrefabDefinition());

        AI_Route_Node newNode = obj.GetComponent<AI_Route_Node>();
        newNode.transform.parent = node.transform.root;

        newNode.transform.SetPositionAndRotation(node.transform.position + node.transform.forward * 50, node.transform.rotation);
        newNode.transform.localScale = node.transform.localScale;

        int ix = node.name.LastIndexOf('_');

        string s = node.name.Substring(ix + 1, node.name.Length - ix - 1);
        int i = int.Parse(s) + 1;

        s = node.name[..(ix + 1)] + i;
        newNode.name = s;
        
        newNode.children.Clear();
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