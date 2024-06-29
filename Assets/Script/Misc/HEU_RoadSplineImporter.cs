using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

using Vector3 = UnityEngine.Vector3;

public struct RoadSplinePointData
{
    public Vector3 position;
    public Vector3 tangent;
    public Vector3 up;
}

[ExecuteInEditMode]
public class HEU_RoadSplineImporter : MonoBehaviour
{
    public string miniMap_Tag_Layer_Name = "MiniMap";

    public class Rootobject
    {
        public Point[] points;
    }

    [Serializable]
    public class Point
    {
        public float x;
        public float y;
        public float z;
        public float n_x;
        public float n_y;
        public float n_z;
        public float up_x;
        public float up_y;
        public float up_z;
    }

    [HideInInspector]
    public SplineContainer splineComp;

    [HideInInspector]
    public Octree octree;

    [HideInInspector]
    public Rootobject ParsedData;

    [HideInInspector]
    public List<Vector3> Positions = new();
    [HideInInspector]
    public List<Vector3> Tangenets = new();
    [HideInInspector]
    public List<Vector3> Ups = new();

    public bool drawDebug = true;

//     void Start()
//     {
//         
//     }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && drawDebug && splineComp)
        {
            for (int i = 0; i < splineComp.Spline.Count; i++)
            {
                Vector3 c = Positions[i];
                Vector3 t = Positions[i] + Tangenets[i] * 2;
                Vector3 u = Positions[i] + Ups[i] * 2;

                Debug.DrawLine(c, t, Color.green);
                Debug.DrawLine(c, u, Color.blue);
            }
        }
#endif
    }

    void Callback(string arg)
    {
#if UNITY_EDITOR

        splineComp = GetComponent<SplineContainer>();

        if (!splineComp)
        {
            splineComp = transform.AddComponent<SplineContainer>();
        }

        ParsedData = JsonUtility.FromJson<Rootobject>(arg);

        splineComp.Spline.EditType = SplineType.Linear;
        splineComp.Spline.Clear();

        Positions.Clear();
        Tangenets.Clear();
        Ups.Clear();

        foreach (Point p in ParsedData.points)
        {
            Vector3 pos = new(-p.x, p.z, p.y);
            Vector3 Tangent = new(-p.n_x, p.n_z, p.n_y);
            Vector3 Up = new(-p.up_x, p.up_z, p.up_y);

            Positions.Add(pos);
            Tangenets.Add(Tangent);
            Ups.Add(Up);

            splineComp.Spline.Add(new BezierKnot(pos));
        }

        EditorUtility.SetDirty(splineComp);
        EditorUtility.SetDirty(this);


        GameObject miniMapMesh = GameObject.FindGameObjectWithTag(miniMap_Tag_Layer_Name);
        miniMapMesh.layer = LayerMask.NameToLayer(miniMap_Tag_Layer_Name);

        MeshRenderer meshRenderer = miniMapMesh.GetComponent<MeshRenderer>();
        meshRenderer.staticShadowCaster = false;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        EditorUtility.SetDirty(miniMapMesh);


        octree = GetComponent<Octree>();

        if (!octree)
        {
            octree = transform.AddComponent<Octree>();
        }

        Vector3 center;
        Vector3 boundary;
        Octree.CalculateCenterAndBoundFromPoints(Positions.ToArray(), out center, out boundary);

        octree.Init(boundary, center, 50);

        EditorUtility.SetDirty(octree);


        Debug.Log(arg);
#endif
    }

    public RoadSplinePointData GetClosestRoadSplinePoint(Vector3 position)
    {
        float3 fpos = new (position.x, position.y, position.z);
        //float3 Nearest;
        //float t;

        SplineUtility.GetNearestPoint<Spline>(splineComp.Spline, fpos, out float3 _1, out float t);

        RoadSplinePointData closest = new();
        t = SplineUtility.ConvertIndexUnit<Spline>(splineComp.Spline, t, PathIndexUnit.Knot);

        int ix1 = Mathf.Clamp(Mathf.FloorToInt(t), 0, Positions.Count);
        int ix2 = Mathf.Clamp(Mathf.CeilToInt(t), 0, Positions.Count);
        float frac = t - ix1;

        closest.position = Vector3.Lerp(Positions[ix1], Positions[ix2], frac);
        closest.tangent = Vector3.Lerp(Tangenets[ix1], Tangenets[ix2], frac);
        closest.up = Vector3.Lerp(Ups[ix1], Ups[ix2], frac);

        return closest;
    }
}
