using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

using Vector3 = UnityEngine.Vector3;

public struct RoadSplinePointDataTemp
{
    public Vector3 position;
    public Vector3 tangent;
    public Vector3 up;
}

[ExecuteInEditMode]
public class HEU_RoadSplineImporter : MonoBehaviour
{
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
    public Rootobject ParsedData;

    [HideInInspector]
    public List<Vector3> Positions = new();
    [HideInInspector]
    public List<Vector3> Tangenets = new();
    [HideInInspector]
    public List<Vector3> Ups = new();

    public bool drawDebug = true;

    void Start()
    {
        
    }

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
            Vector3 pos = new Vector3(p.x, p.y, p.z);
            float y = pos.y;
            pos.y = pos.z;
            pos.z = y;
            pos.x = -pos.x;

            Vector3 Tangent = new Vector3(p.n_x, p.n_y, p.n_z);
            y = Tangent.y;
            Tangent.y = Tangent.z;
            Tangent.z = y;
            Tangent.x = -Tangent.x;

            Vector3 Up = new Vector3(p.up_x, p.up_y, p.up_z);
            y = Up.y;
            Up.y = Up.z;
            Up.z = y;
            Up.x = -Up.x;

            Positions.Add(pos);
            Tangenets.Add(Tangent);
            Ups.Add(Up);

            splineComp.Spline.Add(new BezierKnot(pos));
        }

        EditorUtility.SetDirty(splineComp);
        EditorUtility.SetDirty(this);

        Debug.Log(arg);
#endif
    }

    public RoadSplinePointData GetClosestRoadSplinePoint(Vector3 position)
    {
        float3 fpos = new float3(position.x, position.y, position.z);
        float3 Nearest;
        float t;

        SplineUtility.GetNearestPoint<Spline>(splineComp.Spline, fpos, out Nearest, out t);

        RoadSplinePointData closest = new RoadSplinePointData();
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
