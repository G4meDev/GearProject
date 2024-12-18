using UnityEngine;
using UnityEditor;

public class Camera_Orient : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Camera_Orient))]
public class Camera_Orient_Editor : Editor
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

    public void OnSceneGUI()
    {

    }

    private void CustomOnSceneGUI(SceneView view)
    {
        var t = target as Camera_Orient;
        if (!t)
            return;

        Camera_Orient_Node[] nodes = t.GetComponentsInChildren<Camera_Orient_Node>();

        foreach (Camera_Orient_Node node in nodes)
        {
            foreach(Camera_Orient_Node n in node.Neighbours)
            {
                if(n)
                Handles.DrawLine(node.transform.position, n.transform.position, 10);
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

        Camera_Orient_Node n1 = objs[0].GetComponent<Camera_Orient_Node>();
        Camera_Orient_Node n2 = objs[1].GetComponent<Camera_Orient_Node>();

        if(!n1 || !n2)
        {
            return;
        }

        if(n1.Neighbours.Contains(n2))
        {
            n1.Neighbours.Remove(n2);
            n2.Neighbours.Remove(n1);
        }

        else
        {
            n1.Neighbours.Add(n2);
            n2.Neighbours.Add(n1);
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