using JetBrains.Annotations;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;

    public int targetPos = -1;
    public AI_Position_Params position_params;

    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;

    public Controller_PID steerPID;

    public float targetTrackError = 0.0f;


    //     public float steer_p = 0.15f;
    //     public float steer_i = 0.01f;
    //     public float steer_d = 0.1f;

    public float steer_p = 0.15f;
    public float steer_i = 0.01f;
    public float steer_d = 0.1f;

    private float steer_p_active;
    private float steer_i_active;
    private float steer_d_active;

    private float steer_wind_start;
    private float steer_wind_duration;

    public void UpdateTargetPosition(int newPos)
    {
        targetPos = newPos;

        position_params = AI_Params.GetPositionParams(targetPos);
    }


    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
        aiRouteNode_Current = node;

        UpdateTargetNode();
    }

    public void UpdateTargetNode()
    {
        if (aiRouteNode_Current.children.Count > 0)
        {
            aiRouteNode_Target = aiRouteNode_Current.children[0];

//             float targetScale = aiRouteNode_Target.transform.lossyScale.x / 8;
//             targetTrackError = Random.Range(-targetScale, targetScale);
        }

        else
        {
            aiRouteNode_Current = null;
        }
    }

    public Vector3 GetNearestWorldPosition(out float optimalTrackError, out float trackErrorRange)
    {
        optimalTrackError = 0.0f;
        trackErrorRange = 0.0f;

        if (aiRouteNode_Current && aiRouteNode_Target)
        {
            Vector3 d = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
            Vector3 toPos = vehicle.vehicleProxy.transform.position - aiRouteNode_Current.transform.position;

            float dot = Vector3.Dot(d.normalized, toPos);

            // passed target without hitting collision
            if (dot > d.magnitude)
            {
                OnEnterNewRouteNode(aiRouteNode_Target);

                return GetNearestWorldPosition(out optimalTrackError, out trackErrorRange);
            }

            // should check for parent nodes
            else if (dot < 0)
            {
                AI_Route_Node bestParent = null;
                float min_dist = float.MaxValue;

                foreach(AI_Route_Node parent in aiRouteNode_Current.parents)
                {
                    float dist = Vector3.Distance(vehicle.vehicleProxy.transform.position, parent.transform.position);

                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        bestParent = parent;
                    }
                }



                d = aiRouteNode_Current.transform.position - bestParent.transform.position;
                toPos = vehicle.vehicleProxy.transform.position - bestParent.transform.position;

                dot = Vector3.Dot(d.normalized, toPos);
                dot = Mathf.Clamp(dot, 0, d.magnitude);

                float a = dot / d.magnitude;
                optimalTrackError = Mathf.Lerp(bestParent.optimalCrossSecion, aiRouteNode_Current.optimalCrossSecion, a);
                trackErrorRange = Mathf.Lerp(bestParent.transform.lossyScale.x, aiRouteNode_Current.transform.lossyScale.x, a);

                return bestParent.transform.position + dot * d.normalized;
            }

            else
            {
                float a = dot / d.magnitude;

                optimalTrackError = Mathf.Lerp(aiRouteNode_Current.optimalCrossSecion, aiRouteNode_Target.optimalCrossSecion, a);
                trackErrorRange = Mathf.Lerp(aiRouteNode_Current.transform.lossyScale.x, aiRouteNode_Target.transform.lossyScale.x, a);

                return aiRouteNode_Current.transform.position + dot * d.normalized;
            }
        }

        return Vector3.zero;
    }

    void Start_Steer_Wind(float duration)
    {
        steer_wind_start = Time.time;
        steer_wind_duration = duration;
    }

    void Wind_Steer()
    {
        float alpha = (Time.time - steer_wind_start) / steer_wind_duration;
        if (alpha > 1.0f)
        {
            steer_p_active = steer_p;
            steer_i_active = steer_i;
            steer_d_active = steer_d;
        }
        else
        {
            steer_p_active = Mathf.Lerp(0, steer_p, alpha);
            steer_i_active = Mathf.Lerp(0, steer_i, alpha);
            steer_d_active = Mathf.Lerp(0, steer_d, alpha);
        }

        steerPID.Init(steer_p_active, steer_i_active, steer_d_active);
    }


    void Start()
    {
        vehicle = GetComponent<Vehicle>();
        steerPID = gameObject.AddComponent<Controller_PID>();

        SceneManager.RegisterAI(this);

        steer_p_active = steer_p;
        steer_i_active = steer_i;
        steer_d_active = steer_d;

        steerPID.Init(steer_p_active, steer_i_active, steer_d_active);

        Start_Steer_Wind(20.0f);
    }

    void Update()
    {
        Wind_Steer();

        Vector3 nearestpos = GetNearestWorldPosition(out float optimalTrackError, out float trackErrorRange);

        float dist = Vector3.Distance(nearestpos, vehicle.vehicleProxy.transform.position);

        Vector3 right = Vector3.Cross(Vector3.up, aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position);

        dist = Vector3.Dot(right, vehicle.vehicleProxy.transform.position - nearestpos) > 0 ? dist : -dist;

        float samplePos = Time.time / 5.0f;
        float noise = Mathf.PerlinNoise(name.GetHashCode(), samplePos);

        trackErrorRange /= 1;
        targetTrackError = Mathf.Lerp(-trackErrorRange, trackErrorRange, noise);

        targetTrackError = Mathf.Lerp(targetTrackError, optimalTrackError, position_params.optimalPathChance);
        
        float error = targetTrackError - dist;


        float steer = steerPID.Step(error, Time.deltaTime);
        steerPID.LimitIntegral(5);

        // -------------------------------------------------------------------------------------

        float a;
        if(vehicle.position > targetPos)
        {
            a = 1;
        }
        else if(vehicle.position < targetPos)
        {
            a = 0;
        }
        else
        {
            a = 0.5f;
        }


        float targetSpeed = Mathf.Lerp(position_params.minSpeed, position_params.maxSpeed, a);

        float throttleValue = targetSpeed > vehicle.forwardSpeed || vehicle.forwardSpeed < position_params.minSpeed ? 1 : 0;

        Debug.Log(name + vehicle.position + "_" + targetPos + "_" + throttleValue + "_" + vehicle.forwardSpeed + "/" + targetSpeed);

        if (vehicle)
        {
            vehicle.vInput = throttleValue;

            vehicle.SetSteerInput(steer);

            //Debug.Log(vehicle.hInput);
        }

    }

}
