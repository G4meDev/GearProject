using UnityEngine;
using UnityEditor;

public class AntiGravity : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

[CustomEditor(typeof(AntiGravity))]
public class AntiGravity_Editor : Editor
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
        var t = target as AntiGravity;
        if (!t)
            return;

        AntiGravity_Node[] nodes = t.GetComponentsInChildren<AntiGravity_Node>();

        foreach (AntiGravity_Node node in nodes)
        {
            foreach(AntiGravity_Node n in node.Neighbours)
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

        AntiGravity_Node n1 = objs[0].GetComponent<AntiGravity_Node>();
        AntiGravity_Node n2 = objs[1].GetComponent<AntiGravity_Node>();

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