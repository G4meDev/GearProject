using UnityEngine;
using UnityEditor;

public class Glider : MonoBehaviour
{
    
}

#if UNITY_EDITOR

[CustomEditor(typeof(Glider))]
public class Glider_Editor : Editor
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
        var t = target as Glider;
        if (!t)
            return;

        Glider_Node[] nodes = t.GetComponentsInChildren<Glider_Node>();

        foreach (Glider_Node node in nodes)
        {
            if(node.next)
            {
                Vector3 halfPos = (node.transform.position + node.next.transform.position) / 2;
                Handles.DrawDottedLine(node.transform.position, halfPos, 5);
                Handles.DrawLine(halfPos, node.next.transform.position, 10);
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

        Glider_Node n1 = objs[0].GetComponent<Glider_Node>();
        Glider_Node n2 = objs[1].GetComponent<Glider_Node>();

        if(!n1 || !n2)
        {
            return;
        }

        if(n1.next == n2)
        {
            n1.next = null;
            n2.next = n1;
        }

        else
        {
            n2.next = null;
            n1.next = n2;
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