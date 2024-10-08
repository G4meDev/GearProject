using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class AI_Route : MonoBehaviour
{
    public bool dirty = false;

    private void Update()
    {
        if (dirty == true)
        {
            dirty = false;

            AI_Route_Node[] nodes = GetComponentsInChildren<AI_Route_Node>();
            foreach (AI_Route_Node node in nodes)
            {
                if(node)
                {
                    node.childDist.Clear();

                    for (int i = 0; i < node.children.Count; i++)
                    {
                        AI_Route_Node child = node.children[i];

                        float dist = child ? Vector3.Distance(node.transform.position, child.transform.position) : 0;
                        node.childDist.Add(dist);
                    }
                }
            }
        }
    }
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
    }
}

#endif