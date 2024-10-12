using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class MathHelper
{
    public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
    {
        var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        return localToWorldMatrix.MultiplyPoint3x4(position);
    }


    public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
    {
        //         RigidTransform t = new RigidTransform(transform.rotation, transform.position);
        //         t = math.inverse(t);
        // 
        //         float4 Output = math.mul(t, new float4(position, 1));
        // 
        //         return new Vector3(Output.x, Output.y, Output.z);


        //         NativeArray<float4> output = new NativeArray<float4>(1, Allocator.TempJob);
        // 
        //         var job = new InverseTransformPointUnscaledJob
        //         {
        //             targetPos = transform.position,
        //             targetRot = transform.rotation,
        // 
        //             pos = new float4(position, 1),
        // 
        //             Output = output
        //         };
        // 
        //         job.Run();
        // 
        //         Vector3 result = new Vector3(output[0].x, output[0].y, output[0].z);
        //         output.Dispose();
        // 
        //         return result;

        var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
        return worldToLocalMatrix.MultiplyPoint3x4(position);
    }

//     [BurstCompile(CompileSynchronously = true)]
//     public struct InverseTransformPointUnscaledJob : IJob
//     {
//         [ReadOnly]
//         public float3 targetPos;
// 
//         [ReadOnly]
//         public quaternion targetRot;
// 
//         [ReadOnly]
//         public float4 pos;
// 
//         [WriteOnly]
//         public NativeArray<float4> Output;
// 
//         public void Execute()
//         {
//             RigidTransform t = new RigidTransform(targetRot, targetPos);
//             t = math.inverse(t);
// 
//             Output[0] = math.mul(t, pos);
//         }
//     }
}