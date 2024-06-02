using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public bool markDirty = false;

    public TextAsset TextFile;

    public Rootobject ParsedData;


    void Start()
    {

    }

    void Update()
    {
        if (!Application.isPlaying && markDirty)
        {
            string rawData = TextFile.text;

            ParsedData = JsonUtility.FromJson<Rootobject>(rawData);



            markDirty = false;
        }
    }
}
