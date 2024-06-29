using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

using Random = UnityEngine.Random;

public class Octree : MonoBehaviour
{
    public List<Vector3> points = new();
    
    public Vector3 boundary;
    public Vector3 center;

    public int Capcity = 50;

    public bool divided = false;

    public Color randColor;

    public MaterialPropertyBlock matBlock;

    // from top face, north west, clockwise

    public GameObject child_1;
    public GameObject child_2;
    public GameObject child_3;
    public GameObject child_4;
    public GameObject child_5;
    public GameObject child_6;
    public GameObject child_7;
    public GameObject child_8;

    public Octree octree_1;
    public Octree octree_2;
    public Octree octree_3;
    public Octree octree_4;
    public Octree octree_5;
    public Octree octree_6;
    public Octree octree_7;
    public Octree octree_8;

    public static Mesh unitPreviewBox;

    public static void CalculateCenterAndBoundFromPoints(Vector3[] inPoints, out Vector3 center, out Vector3 boundary)
    {
        float min_x = float.MaxValue;
        float max_x = float.MinValue;

        float min_y = float.MaxValue;
        float max_y = float.MinValue;

        float min_z = float.MaxValue;
        float max_z = float.MinValue;

        foreach(Vector3 pos in inPoints)
        {
            min_x = Mathf.Min(min_x, pos.x);
            max_x = Mathf.Max(max_x, pos.x);

            min_y = Mathf.Min(min_y, pos.y);
            max_y = Mathf.Max(max_y, pos.y);

            min_z = Mathf.Min(min_z, pos.z);
            max_z = Mathf.Max(max_z, pos.z);
        }

        boundary = new(max_x - min_x, max_y - min_y, max_z - min_z);
        center = new Vector3(min_x, min_y, min_z) + boundary / 2;
    }

    bool IsPointInBoundary(Vector3 point, Vector3 boundary, Vector3 center)
    {
        Vector3 d = point - center;

        return boundary.x / 2 >= Mathf.Abs(d.x)
            && boundary.y / 2 >= Mathf.Abs(d.y) 
            && boundary.z / 2 >= Mathf.Abs(d.z);
    }

    public bool Insert(Vector3 point)
    {
        if (!IsPointInBoundary(point, boundary, center))
            return false;

        if(divided)
        {
            if (octree_1.Insert(point))
                return true;
            if (octree_2.Insert(point))
                return true;
            if (octree_3.Insert(point))
                return true;
            if (octree_4.Insert(point))
                return true;
            if (octree_5.Insert(point))
                return true;
            if (octree_6.Insert(point))
                return true;
            if (octree_7.Insert(point))
                return true;
            if (octree_8.Insert(point))
                return true;

            return false;
        }


        points.Add(point);

        if (points.Count == Capcity)
            Subdivide();

        return true;
    }

    private void Subdivide()
    {
        if (!divided)
        {
            divided = true;

            Vector3 halfBoundary = boundary / 2;
            Vector3 quadBoundary = halfBoundary / 2;

            child_1 = new GameObject("1");
            octree_1 = child_1.AddComponent<Octree>();
            child_1.transform.SetParent(transform);

            child_2 = new GameObject("2");
            octree_2 = child_2.AddComponent<Octree>();
            child_2.transform.SetParent(transform);

            child_3 = new GameObject("3");
            octree_3 = child_3.AddComponent<Octree>();
            child_3.transform.SetParent(transform);

            child_4 = new GameObject("4");
            octree_4 = child_4.AddComponent<Octree>();
            child_4.transform.SetParent(transform);

            child_5 = new GameObject("5");
            octree_5 = child_5.AddComponent<Octree>();
            child_5.transform.SetParent(transform);

            child_6 = new GameObject("6");
            octree_6 = child_6.AddComponent<Octree>();
            child_6.transform.SetParent(transform);

            child_7 = new GameObject("7");
            octree_7 = child_7.AddComponent<Octree>();
            child_7.transform.SetParent(transform);

            child_8 = new GameObject("8");
            octree_8 = child_8.AddComponent<Octree>();
            child_8.transform.SetParent(transform);


            octree_1.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(1, 1, -1)),     Capcity);
            octree_2.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(1, 1, 1)),      Capcity);
            octree_3.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(-1, 1, -1)),    Capcity);
            octree_4.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(-1, 1, 1)),     Capcity);
            octree_5.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(1, -1, -1)),    Capcity);
            octree_6.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(1, -1, 1)),     Capcity);
            octree_7.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(-1, -1, -1)),   Capcity);
            octree_8.Init(halfBoundary, center + Vector3.Scale(quadBoundary, new Vector3(-1, -1, 1)),    Capcity);

            foreach(Vector3 point in points)
            {
                if (octree_1.Insert(point))
                    continue;
                if (octree_2.Insert(point))
                    continue;
                if (octree_3.Insert(point))
                    continue;
                if (octree_4.Insert(point))
                    continue;
                if (octree_5.Insert(point))
                    continue;
                if (octree_6.Insert(point))
                    continue;
                if (octree_7.Insert(point))
                    continue;
                if (octree_8.Insert(point))
                    continue;
            }

            points.Clear();
        }
    }


    public void Init(Vector3 inBoundary, Vector3 inCenter, int inCapcity)
    {
        boundary = inBoundary;
        center = inCenter;
        Capcity = inCapcity;

    }

    public void PostEdit()
    {
        if (!unitPreviewBox)
            unitPreviewBox = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        Octree[] comps = transform.GetComponentsInChildren<Octree>();

        foreach(Octree comp in comps)
        {
            if(!comp.divided)
            {
                comp.transform.position = comp.center;
                comp.transform.localScale = comp.boundary;

                MeshFilter meshFilter = comp.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = unitPreviewBox;

                MeshRenderer meshRenderer = comp.AddComponent<MeshRenderer>();

                comp.randColor = Random.ColorHSV();

                Material material = Resources.Load<Material>("OctreePreviewMaterial");
                material.enableInstancing = true;

                meshRenderer.material = material;
                meshRenderer.material.SetColor("_Color", comp.randColor);
            }
        }
    }


}