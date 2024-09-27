using UnityEngine;
using UnityEditor;

public class AI_Route : MonoBehaviour
{
    
}

#if UNITY_EDITOR

[CustomEditor(typeof(AI_Route))]
public class AI_Route_Editor : Editor
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
        var t = target as AI_Route;
        if (!t)
            return;

        AI_Route_Node[] nodes = t.GetComponentsInChildren<AI_Route_Node>();

        foreach (AI_Route_Node node in nodes)
        {
            foreach (AI_Route_Node child in node.children)
            {
                if (child)
                {
                    Vector3 halfPos = (node.transform.position + child.transform.position) / 2;
                    Handles.DrawDottedLine(node.transform.position, halfPos, 5);
                    Handles.DrawLine(halfPos, child.transform.position, 10);
                }
            }
        }

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

        AI_Route_Node n1 = objs[0].GetComponent<AI_Route_Node>();
        AI_Route_Node n2 = objs[1].GetComponent<AI_Route_Node>();

        if(!n1 || !n2)
        {
            return;
        }

        if(n1.children.Contains(n2))
        {
            n1.children.Remove(n2);
            
            if(!n2.children.Contains(n1))
            {
                n2.children.Add(n1);
            }
        }

        else
        {
            n1.children.Add(n2);

            if(n2.children.Contains(n1))
            {
                n2.children.Remove(n1);
            }
        }

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