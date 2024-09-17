using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class PullPath : MonoBehaviour
{
    public SplineContainer spline;

    public float innerRadius = 5.0f;
    public float outerRadius = 10.0f;
    public float falloff = 1.0f;
    public float strength = 10.0f;

    public bool bDrawDebug = true;
    public int debugSteps = 20;

    public Vector3 GetForceAtPosition(Vector3 pos)
    {
        if (spline)
        {
            float t;
            float3 nearest;
            float3 tangent;

            pos = spline.transform.InverseTransformPoint(pos);

            float dist = SplineUtility.GetNearestPoint<Spline>(spline.Spline, new float3(pos.x, pos.y, pos.z), out nearest, out t, 1, 1);
            tangent = SplineUtility.EvaluateTangent<Spline>(spline.Spline, t);

            float alpha = Mathf.Clamp01((dist - innerRadius) / (outerRadius - innerRadius));
            alpha = Mathf.Pow(1 - alpha, falloff);

            //Debug.Log(alpha);
            
            return Vector3.Normalize(new Vector3(tangent.x, tangent.y, tangent.z))* alpha * strength;
        }

        return Vector3.zero;
    }

    void Start()
    {
        
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && bDrawDebug && spline)
        {
            //float len = spline.Spline.leng();

            float step = 1.0f / debugSteps;

            for (float i = 0; i < 1; i+=step)
            {
                Vector3 p = spline.EvaluatePosition(i);

                DrawHelpers.DrawSphere(p, innerRadius, Color.red);
                DrawHelpers.DrawSphere(p, outerRadius, Color.blue);
            }

        }

#endif
    }
}
