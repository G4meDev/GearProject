using UnityEngine;

public static class MeshHelpers
{
    public static Vector3 GetSmoothNormalFromHit(ref RaycastHit hit)
    {
        MeshCollider meshCollider = hit.collider as MeshCollider;

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;

        Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];

        Vector3 baryCenter = hit.barycentricCoordinate;

        Vector3 SmoothNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
        SmoothNormal = SmoothNormal.normalized;

        Transform hitTransform = hit.collider.transform;
        SmoothNormal = hitTransform.TransformDirection(SmoothNormal);

        return SmoothNormal;
    }
}
