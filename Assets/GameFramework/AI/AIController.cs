using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;

    public int targetPos = -1;
    public AI_Position_Params position_params;

    public AI_Route_Node aiRouteNode_Current;
    public AI_Route_Node aiRouteNode_Target;


    public float targetTrackError = 0.0f;

    //------------------------------------------------------------

    public Controller_PID steerPID;

    public float steer_p = 0.15f;
    public float steer_i = 0.01f;
    public float steer_d = 0.1f;

    private float steer_p_active;
    private float steer_i_active;
    private float steer_d_active;

    private float steer_wind_start;
    private float steer_wind_duration;

    //------------------------------------------------------------

    public Controller_PID driftPID;

    public float drift_p = 1.0f;
    public float drift_i = 0.0f;
    public float drift_d = 0.8f;

    //------------------------------------------------------------

    public Controller_PID throttlePID;

    public float throttle_p = 20.0f;
    public float throttle_i = 0.8f;
    public float throttle_d = 0.35f;

    //------------------------------------------------------------

    private bool driftable = false;
    private int driftDir = 0;

    public void OnKilled()
    {
        driftable = false;
        driftDir = 0;
        vehicle.holdingJump = false;

        steerPID.LimitIntegral(0);
        driftPID.LimitIntegral(0);
        throttlePID.LimitIntegral(0);

        Start_Steer_Wind(10.0f);
    }

    public void UpdateTargetPosition(int newPos)
    {
        targetPos = newPos;

        position_params = AI_Params.GetPositionParams(targetPos);
    }


    public void OnEnterNewRouteNode(AI_Route_Node node)
    {
        AI_Route_Node prevNode = aiRouteNode_Current;

        aiRouteNode_Current = node;

        UpdateTargetNode();

        if (vehicle && !vehicle.isDrifting())
        {
            driftDir = aiRouteNode_Current.GetDriftDirectionToTarget(aiRouteNode_Target);
            if (driftDir != 0)
                driftable = true;
        }

        // moving in drift node with diffrent dir
        else if (vehicle && vehicle.isDrifting())
        {
            int dir = aiRouteNode_Current.GetDriftDirectionToTarget(aiRouteNode_Target);

            if (dir != 0 && dir != driftDir)
            {
                driftDir = dir;
                //vehicle.dr = false;
                vehicle.holdingJump = false;
                driftable = true;
            }
        }

    }

    public void TryDrifting()
    {
        //         if(vehicle.forwardSpeed > vehicle.minDriftSpeed)
        //         {
        //             Debug.Log(vehicle.name + "   try drifting!");
        // 
        //             bWantsToDrift = true;
        //         }
    }

    public void UpdateTargetNode()
    {
        if (aiRouteNode_Current.children.Count > 0)
        {
            //aiRouteNode_Target = aiRouteNode_Current.children[0];

            //             float targetScale = aiRouteNode_Target.transform.lossyScale.x / 8;
            //             targetTrackError = Random.Range(-targetScale, targetScale);
        }

        else
        {
            aiRouteNode_Target = null;
        }
    }

    public Vector3 GetNearestWorldPosition(out float optimalTrackError, out float trackErrorRange, out Vector3 tan)
    {
        optimalTrackError = 0.0f;
        trackErrorRange = 0.0f;
        tan = Vector3.zero;

        if (aiRouteNode_Current && aiRouteNode_Target)
        {
            Vector3 d = aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position;
            Vector3 toPos = vehicle.vehicleProxy.transform.position - aiRouteNode_Current.transform.position;

            float dot = Vector3.Dot(d.normalized, toPos);

            // passed target without hitting collision
            if (dot > d.magnitude)
            {
                OnEnterNewRouteNode(aiRouteNode_Target);

                return GetNearestWorldPosition(out optimalTrackError, out trackErrorRange, out tan);
            }

            // should check for parent nodes
            else if (dot < 0)
            {
                AI_Route_Node bestParent = null;
                float min_dist = float.MaxValue;

                foreach (AI_Route_Node parent in aiRouteNode_Current.parents)
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
                trackErrorRange = Mathf.Lerp(bestParent.transform.lossyScale.x, aiRouteNode_Current.transform.lossyScale.x, a);
                tan = Vector3.Lerp(bestParent.transform.forward, aiRouteNode_Current.transform.forward, a);

                return bestParent.transform.position + dot * d.normalized;
            }

            else
            {
                float a = dot / d.magnitude;

                trackErrorRange = Mathf.Lerp(aiRouteNode_Current.transform.lossyScale.x, aiRouteNode_Target.transform.lossyScale.x, a);
                tan = Vector3.Lerp(aiRouteNode_Current.transform.forward, aiRouteNode_Target.transform.forward, a);

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
        throttlePID = gameObject.AddComponent<Controller_PID>();
        driftPID = gameObject.AddComponent<Controller_PID>();

        vehicle.killDelegate = OnKilled;

        //SceneManager.RegisterAI(this);

        steer_p_active = steer_p;
        steer_i_active = steer_i;
        steer_d_active = steer_d;

        steerPID.Init(steer_p_active, steer_i_active, steer_d_active);

        Start_Steer_Wind(20.0f);


        driftPID.Init(drift_p, drift_i, drift_d);

        throttlePID.Init(throttle_p, throttle_i, throttle_d);
    }

    void Update()
    {
        vehicle.vInput = 1;
        return;



        bool rubberBanding = position_params.rubberBadingDist < vehicle.distanceFromFirstPlace;

        Vector3 nearestpos = GetNearestWorldPosition(out float optimalTrackError, out float trackErrorRange, out Vector3 tan);
        float dist = Vector3.Distance(nearestpos, vehicle.vehicleProxy.transform.position);

        Vector3 right = Vector3.Cross(Vector3.up, aiRouteNode_Target.transform.position - aiRouteNode_Current.transform.position);
        dist = Vector3.Dot(right, vehicle.vehicleProxy.transform.position - nearestpos) > 0 ? dist : -dist;

        float samplePos = Time.time / 5.0f;
        float noise = Mathf.PerlinNoise(name.GetHashCode(), samplePos);

        trackErrorRange /= 1;
        targetTrackError = Mathf.Lerp(-trackErrorRange, trackErrorRange, noise);

        float optChance = rubberBanding ? Mathf.Clamp01(position_params.optimalPathChance + AI_Params.rbOptimalPathChanceIncrease) : position_params.optimalPathChance;
        targetTrackError = Mathf.Lerp(targetTrackError, optimalTrackError, optChance);

        float steerError = targetTrackError - dist;


        float distanceFromRoadEdge = trackErrorRange / 2 - Mathf.Abs(dist);

        if (distanceFromRoadEdge < AI_Params.driftHaltDistanceToRoadEdge && vehicle.isDrifting())
        {
            // end drifting

            //Debug.Log("end drift!");

            vehicle.EndDrift();
            vehicle.holdingJump = false;
        }


        // -------------------------------------------------------------------------------------

        float steer = 0;

        Debug.DrawRay(vehicle.vehicleProxy.transform.position + Vector3.up * 5, tan * 5, Color.blue);
        Debug.DrawRay(vehicle.vehicleProxy.transform.position + Vector3.up * 5, vehicle.vehicleProxy.velocity.normalized * 5, Color.red);

        if (vehicle.isDrifting())
        {
            Vector3 veloDir = vehicle.vehicleProxy.velocity.normalized;

            float dot = Vector3.Dot(tan, veloDir);
            //@TODO: bake data
            Vector3 nodeRight = Vector3.Cross(Vector3.up, tan);
            float sign = Mathf.Sign(Vector3.Dot(veloDir, nodeRight));

            dot = 1 - dot;

            if (dot > AI_Params.maxDriftHaltAngleAlpha)
            {
                //Debug.Log("end drift!");

                vehicle.EndDrift();
                vehicle.holdingJump = false;
            }
            else
            {
                dot *= sign;
                //Debug.Log(dot);

                steer = driftPID.Step(-dot, Time.deltaTime);
            }
        }

        else
        {
            Wind_Steer();

            steer = steerPID.Step(steerError, Time.deltaTime);
            steerPID.LimitIntegral(5);
        }

        // -------------------------------------------------------------------------------------

        float a;
        if (vehicle.position > targetPos)
        {
            a = 1;
        }
        else if (vehicle.position < targetPos)
        {
            a = 0;
        }
        else
        {
            a = 0.5f;
        }

        float targetSpeed = Mathf.Lerp(position_params.minSpeed, position_params.maxSpeed, a);
        targetSpeed = rubberBanding ? targetSpeed + AI_Params.rbSpeedIncrease : targetSpeed;

        float throttleError = targetSpeed - vehicle.forwardSpeed;

        float throttleValue = Mathf.Clamp01(throttlePID.Step(throttleError, Time.deltaTime));
        throttlePID.LimitIntegral(2);

        // -------------------------------------------------------------------------------------

        //bool bShouldDrift = targetSpeed > vehicle.GetMaxSpeedWithModifiers();

        if (vehicle.isDrifting())
        {
            //Debug.Log(steer);
        }

        if (vehicle)
        {
            vehicle.SetThrottleInput(throttleValue);
            vehicle.SetSteerInput(steer);

            if (driftable && !vehicle.isDrifting() && vehicle.forwardSpeed > vehicle.minDriftSpeed)
            {
                vehicle.SetSteerInput(driftDir);

                vehicle.StartDrift();
                vehicle.holdingJump = true;
                driftPID.LimitIntegral(0);
            }
            else
            {
                driftable = false;
            }

        }

    }

}