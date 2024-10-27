using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LapPath : MonoBehaviour
{
    public float maxNodeIndex = -1;

    public List<float> distanceList;

    public bool dirty = false;


    public float GetFullLapDistance()
    {
        return distanceList[distanceList.Count - 1];
    }

    public void RegenData()
    {

#if UNITY_EDITOR
        distanceList.Clear();

        LapPath_Node node = null;

        LapPath_Node[] nodes = GetComponentsInChildren<LapPath_Node>();
        foreach (LapPath_Node child in nodes)
        {
            if (child.isStart == true)
            {
                node = child;
                node.Dist = 0;
                node.nodeIndex = 0;
                break;
            }
        }

        //@TODO: add multi path support

        distanceList.Add(0);

        while(true)
        {
            LapPath_Node child = node.children[0];

            child.Dist = node.Dist + Vector3.Distance(node.transform.position, child.transform.position);
            distanceList.Add(child.Dist);

            if(child.isStart)
            {
                maxNodeIndex = node.nodeIndex;
                child.Dist = 0;

                break;
            }

            child.nodeIndex = node.nodeIndex + 1;
            node = child;
        }


        foreach (LapPath_Node child in nodes)
        {
            EditorUtility.SetDirty(child);
        }
#endif

    }

    private void Start()
    {

    }

    private void Update()
    {
#if UNITY_EDITOR
        if (dirty == true)
        {
            dirty = false;

            RegenData();
        }
#endif
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LapPath))]
public class LapPath_Editor : Editor
{
    public bool cDown = false;

    private void OnEnable()
    {
        SceneView.duringSceneGui += CustomOnSceneGUI;
    }

//     private void OnDisable()
//     {
//         SceneView.duringSceneGui -= CustomOnSceneGUI;
//     }

    private void CustomOnSceneGUI(SceneView view)
    {
        var t = target as LapPath;
        if (!t)
            return;

        LapPath_Node[] nodes = t.GetComponentsInChildren<LapPath_Node>();

//         foreach (AI_Route_Node node in nodes)
//         {
//             foreach (AI_Route_Node child in node.children)
//             {
//                 if (child)
//                 {
//                     Vector3 halfPos = (node.transform.position + child.transform.position) / 2;
//                     Handles.DrawDottedLine(node.transform.position, halfPos, 5);
//                     Handles.DrawLine(halfPos, child.transform.position, 10);
//                 }
//             }
// 
//             foreach(AI_Route_Node parent in node.parents)
//             {
//                 if(parent)
//                 {
//                     Vector3 offset = Vector3.up * 5;
// 
//                     Vector3 halfPos = (node.transform.position + parent.transform.position) / 2;
//                     Handles.DrawDottedLine(node.transform.position + offset, halfPos + offset, 5);
//                     Handles.DrawLine(halfPos + offset, parent.transform.position + offset, 10);
//                 }
//             }
//         }

        //---------------------------------------------------------------------

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.C && !cDown)
        {
            cDown = true;

            OnConnect();
        }

        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.C)
        {
            cDown = false;
        }

    }

    private void OnConnect()
    {
        GameObject[] objs = Selection.gameObjects;

        if(objs.Length != 2)
        {
            return;
        }

        LapPath_Node n1 = objs[0].GetComponent<LapPath_Node>();
        LapPath_Node n2 = objs[1].GetComponent<LapPath_Node>();

        if(!n1 || !n2)
        {
            return;
        }

        if (!n1.children.Contains(n2))
            (n1, n2) = (n2, n1);

        n1.children.Remove(n2);
        n2.parents.Remove(n1);

        if (!n1.parents.Contains(n2))
            n1.parents.Add(n2);

        if (!n2.children.Contains(n1))
            n2.children.Add(n1);

        n1.OnNeighboursChanged();
        n1.OnNeighboursChanged();

        EditorUtility.SetDirty(n1);
        EditorUtility.SetDirty(n2);

//         foreach (GameObject obj in objs)
//         {
//             Debug.Log(obj.name);
//         }
    }
}

#endif