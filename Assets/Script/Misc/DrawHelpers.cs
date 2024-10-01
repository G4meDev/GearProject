using System;
using UnityEngine;

public static class DrawHelpers
{
    private static readonly Vector4[] s_UnitSphere = MakeUnitSphere(16);

    private static readonly Vector3[] s_UnitCircle = MakeUnitCircle(16);

    // Square with edge of length 1
    private static readonly Vector4[] s_UnitSquare =
    {
            new Vector4(-0.5f,0,   0.5f,  1),
            new Vector4(0.5f,0,   0.5f,  1),
            new Vector4(0.5f,0,   -0.5f,  1),
            new Vector4(-0.5f,0,  -0.5f,  1),
    };

    private static Vector3[] MakeUnitCircle(int len)
    {
        var v = new Vector3[len];
        for (int i = 0; i < len; i++)
        {
            var f = i / (float)len;
            float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
            float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
            v[i].x = c;
            v[i].y = 0.0f;
            v[i].z = s;
        }
        return v;
    }

    private static Vector4[] MakeUnitSphere(int len)
    {
        Debug.Assert(len > 2);
        var v = new Vector4[len * 3];
        for (int i = 0; i < len; i++)
        {
            var f = i / (float)len;
            float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
            float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
            v[0 * len + i] = new Vector4(c, s, 0, 1);
            v[1 * len + i] = new Vector4(0, c, s, 1);
            v[2 * len + i] = new Vector4(s, 0, c, 1);
        }
        return v;
    }

    public static void DrawSphere(Vector4 pos, float radius, Color color)
    {
#if UNITY_EDITOR

        Vector4[] v = s_UnitSphere;
        int len = s_UnitSphere.Length / 3;
        for (int i = 0; i < len; i++)
        {
            var sX = pos + radius * v[0 * len + i];
            var eX = pos + radius * v[0 * len + (i + 1) % len];
            var sY = pos + radius * v[1 * len + i];
            var eY = pos + radius * v[1 * len + (i + 1) % len];
            var sZ = pos + radius * v[2 * len + i];
            var eZ = pos + radius * v[2 * len + (i + 1) % len];
            Debug.DrawLine(sX, eX, color);
            Debug.DrawLine(sY, eY, color);
            Debug.DrawLine(sZ, eZ, color);
        }

#endif
    }

    public static void DrawBox(Vector3 Position, Quaternion Rotation, float Scale, Color color)
    {
#if UNITY_EDITOR

        for (int i = 0; i < s_UnitSquare.Length; i++)
        {
            Vector3 p1 = s_UnitSquare[i];
            Vector3 p2 = s_UnitSquare[(i + 1) % s_UnitSquare.Length];

            p1 *= Scale;
            p2 *= Scale;

            p1 = Rotation * p1;
            p2 = Rotation * p2;

            p1 += Position;
            p2 += Position;

            Debug.DrawLine(p1, p2, color);
        }


#endif
    }

    public static void DrawCircle(Vector3 Position, Quaternion Rotation, float Radius, Color color)
    {
#if UNITY_EDITOR

        for (int i = 0; i < s_UnitCircle.Length; i++)
        {
            Vector3 p1 = s_UnitCircle[i];
            Vector3 p2 = s_UnitCircle[(i + 1) % s_UnitCircle.Length];

            p1 *= Radius;
            p2 *= Radius;
            
            p1 = Rotation * p1;
            p2 = Rotation * p2;

            p1 += Position;
            p2 += Position;

            Debug.DrawLine(p1, p2, color);
        }

#endif
    }

    public static void DrawCylinder(Vector3 Position, Quaternion Rotation, float Radius, float Height, Color color)
    {
#if UNITY_EDITOR

            DrawCircle(Position, Rotation, Radius, color);
            Vector3 dir = Vector3.up;
            Vector3 EndPosition = Position + (Rotation * dir) * Height;
            DrawCircle(EndPosition, Rotation, Radius, color);

            for (int i = 0; i < s_UnitCircle.Length; i++)
            {
                Vector3 p1 = s_UnitCircle[i];

                p1 *= Radius;
                p1 = Rotation * p1;
                p1 += Position;

                Vector3 p2 = p1 + (Rotation * dir) * Height;
                Debug.DrawLine(p1, p2, color);
            }
#endif
    }



    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, int fontSize = 12, Color? colour = null)
    {

#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        GUIStyle label2 = new GUIStyle(GUI.skin.label);
        label2.fontSize = fontSize;
        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text, label2);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
#endif
    }

    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
    }

}


// -----------------------------------------------------
public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
#if UNITY_EDITOR

        Debug.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);

#endif

    }
    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
#if UNITY_EDITOR

        Debug.DrawRay(pos, direction, color);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);

#endif
    }

}

