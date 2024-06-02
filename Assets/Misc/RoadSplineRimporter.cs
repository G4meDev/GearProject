using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class RoadSplineRimporter : MonoBehaviour
{
    [Serializable]
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
    public SplineContainer spline;

    public bool markDirty = false;
    public bool splineVissibility = false;
    [HideInInspector]
    public bool lastVis = false;

    public TextAsset TextFile;

    public Rootobject ParsedData;


    void Start()
    {

    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (markDirty)
            {
                string rawData = TextFile.text;
                ParsedData = JsonUtility.FromJson<Rootobject>(rawData);

                spline = GetComponent<SplineContainer>();

                spline.Spline.EditType = SplineType.Linear;
                spline.Spline.Clear();

                GizmoUtility.SetGizmoEnabled(typeof(SplineContainer), false, true);

                foreach (Point p in ParsedData.points)
                {
                    Vector3 pos = new Vector3(p.x, p.y, p.z);
                    float y = pos.y;
                    pos.y = pos.z;
                    pos.z = y;
                    pos *= 10;

                    spline.Spline.Add(new BezierKnot(pos));
                }

                markDirty = false;
            }
            if(lastVis != splineVissibility)
            {
                Type type = typeof(SplineContainer);
                GizmoUtility.SetGizmoEnabled(type, splineVissibility, true);
                lastVis = splineVissibility;
            }
            
        }
    }
}
