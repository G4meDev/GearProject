using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Octree : MonoBehaviour
{
    //public Vector3[] points;
    
    public Vector3 boundary;
    public Vector3 center;

    public int Capcity = 50;

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

    public void Insesrt(Vector3 point)
    {
        //if()
    }


       

    public void Init(Vector3 inBoundary, Vector3 inCenter, int inCapcity)
    {
        boundary = inBoundary;
        center = inCenter;
        Capcity = inCapcity;

        if(!unitPreviewBox)
            unitPreviewBox = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        GameObject newGameObject = new GameObject("n");
        newGameObject.transform.SetParent(transform);
        
        newGameObject.transform.position = center;
        newGameObject.transform.localScale = boundary;
        
        MeshFilter meshFilter = newGameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = unitPreviewBox;
        
        MeshRenderer meshRenderer = newGameObject.AddComponent<MeshRenderer>();
        
        Material material = Resources.Load<Material>("OctreePreviewMaterial");
        meshRenderer.sharedMaterial = material;
    }


}
