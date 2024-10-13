using UnityEngine;

public class AIController : MonoBehaviour
{
    public Vehicle vehicle;
    public AIRoutePlanning routePlanning;

    public int targetPos = -1;
    public AI_Position_Params position_params;

    public float targetTrackError = 0.0f;

    //------------------------------------------------------------

    public Controller_PID steerPID_CrossTrack;
    public Controller_PID steerPID_Projection_1;
    public Controller_PID steerPID_Projection_2;
    public Controller_PID steerPID_Projection_3;

    public float steer_p = 0.15f;
    public float steer_i = 0.01f;
    public float steer_d = 0.1f;

    public float crossTrackWeight = 1.0f;
    public float projection_1_Weight = 0.4f;
    public float projection_2_Weight = 0.3f;
    public float projection_3_Weight = 0.2f;

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


    private Vector2 crossTrackLocal = Vector2.zero;
    private float crossTrackScale = 20.0f;

    private Vector2 p_1_Local = Vector2.zero;
    private float p_1_Scale = 20.0f;

    private Vector2 p_2_Local = Vector2.zero;
    private float p_2_Scale = 20.0f;

    private Vector2 p_3_Local = Vector2.zero;
    private float p_3_Scale = 20.0f;

    private float changedDist = 0.0f;

    private float localDivide = AI_Params.projection_3_dist * 1.5f;
    private float scaleDivide = 60.0f;

    private float driftLastPressTime = float.MinValue;
    private float driftPressDuration = 0.5f;

    Vector2 localSpeed2D;

    private void UpdateRoute()
    {
        Vector3 temp;

        if (routePlanning.projectionData != null)
        {
            Vector3 crossTrackPos = Vector3.Lerp(routePlanning.projectionData.crossTrackProjection.parent.transform.position,
                routePlanning.projectionData.crossTrackProjection.child.transform.position,
                routePlanning.projectionData.crossTrackProjection.t);

            temp = vehicle.vehicleBox.transform.InverseTransformPointUnscaled(crossTrackPos);
            crossTrackLocal = new Vector2(temp.x, temp.z);

            crossTrackScale = Mathf.Lerp(routePlanning.projectionData.crossTrackProjection.parent.transform.lossyScale.x,
                routePlanning.projectionData.crossTrackProjection.child.transform.lossyScale.x,
                routePlanning.projectionData.crossTrackProjection.t);

            // -------------------------------------------------------------------------------------------------------------------------------------------

            Vector3 projection_1_Pos = Vector3.Lerp(routePlanning.projectionData.Projection_1.parent.transform.position,
                routePlanning.projectionData.Projection_1.child.transform.position,
                routePlanning.projectionData.Projection_1.t);

            temp = vehicle.vehicleBox.transform.InverseTransformPointUnscaled(projection_1_Pos);
            p_1_Local = new Vector2(temp.x, temp.z);

            p_1_Scale = Mathf.Lerp(routePlanning.projectionData.Projection_1.parent.transform.lossyScale.x,
                routePlanning.projectionData.Projection_1.child.transform.lossyScale.x,
                routePlanning.projectionData.Projection_1.t);

            // -------------------------------------------------------------------------------------------------------------------------------------------

            Vector3 projection_2_Pos = Vector3.Lerp(routePlanning.projectionData.Projection_2.parent.transform.position,
                routePlanning.projectionData.Projection_2.child.transform.position,
                routePlanning.projectionData.Projection_2.t);

            temp = vehicle.vehicleBox.transform.InverseTransformPointUnscaled(projection_2_Pos);
            p_2_Local = new Vector2(temp.x, temp.z);

            p_2_Scale = Mathf.Lerp(routePlanning.projectionData.Projection_2.parent.transform.lossyScale.x,
                routePlanning.projectionData.Projection_2.child.transform.lossyScale.x,
                routePlanning.projectionData.Projection_2.t);

            // -------------------------------------------------------------------------------------------------------------------------------------------

            Vector3 projection_3_Pos = Vector3.Lerp(routePlanning.projectionData.Projection_3.parent.transform.position,
                routePlanning.projectionData.Projection_3.child.transform.position,
                routePlanning.projectionData.Projection_3.t);

            temp = vehicle.vehicleBox.transform.InverseTransformPointUnscaled(projection_3_Pos);
            p_3_Local = new Vector2(temp.x, temp.z);

            p_3_Scale = Mathf.Lerp(routePlanning.projectionData.Projection_3.parent.transform.lossyScale.x,
                routePlanning.projectionData.Projection_3.child.transform.lossyScale.x,
                routePlanning.projectionData.Projection_3.t);


            changedDist = routePlanning.projectionData.changedDist;

#if UNITY_EDITOR

            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(name.GetHashCode());
            Color color = UnityEngine.Random.ColorHSV();
            UnityEngine.Random.state = state;

//             DrawHelpers.DrawSphere(crossTrackPos, 3, color);
//             DrawHelpers.DrawSphere(projection_1_Pos, 3, color);
//             DrawHelpers.DrawSphere(projection_2_Pos, 3, color);
//             DrawHelpers.DrawSphere(projection_3_Pos, 3, color);
            // 
            //             DrawHelpers.DrawSphere(crossTrackLocal, 3, color);
            //             DrawHelpers.DrawSphere(p_1_Local, 3, color);
            //             DrawHelpers.DrawSphere(p_2_Local, 3, color);
            //             DrawHelpers.DrawSphere(p_3_Local, 3, color);

#endif
        }
    }


    public void OnKilled()
    {
        steerPID_CrossTrack.LimitIntegral(0);
        steerPID_Projection_1.LimitIntegral(0);
        steerPID_Projection_2.LimitIntegral(0);
        steerPID_Projection_3.LimitIntegral(0);

        driftPID.LimitIntegral(0);
        throttlePID.LimitIntegral(0);
    }

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
        routePlanning = GetComponent<AIRoutePlanning>();

        steerPID_CrossTrack = gameObject.AddComponent<Controller_PID>();
        steerPID_Projection_1 = gameObject.AddComponent<Controller_PID>();
        steerPID_Projection_2 = gameObject.AddComponent <Controller_PID>();
        steerPID_Projection_3 = gameObject.AddComponent<Controller_PID>();

        throttlePID = gameObject.AddComponent<Controller_PID>();
        driftPID = gameObject.AddComponent<Controller_PID>();

        vehicle.killDelegate = OnKilled;

        //SceneManager.RegisterAI(this);

        steerPID_CrossTrack.Init(steer_p, steer_i, steer_d);
        steerPID_Projection_1.Init(steer_p, steer_i, steer_d);
        steerPID_Projection_2.Init(steer_p, steer_i, steer_d);
        steerPID_Projection_3.Init(steer_p, steer_i, steer_d);

        driftPID.Init(drift_p, drift_i, drift_d);
        throttlePID.Init(throttle_p, throttle_i, throttle_d);
    }

    void FixedUpdate()
    {
        UpdateRoute();

        //steerPID_CrossTrack.LimitIntegral(1);
        steerPID_Projection_1.LimitIntegral(2);
        steerPID_Projection_1.LimitIntegral(4);
        steerPID_Projection_1.LimitIntegral(10);

        if (vehicle.aeroState == VehicleAeroState.Gliding)
        {
            vehicle.SetThrottleInput(0);
            vehicle.SetSteerInput(0);
        }

        else
        {
            float throttleError = vehicle.targetSpeed - vehicle.forwardSpeed;
            float throttle = throttlePID.Step(throttleError, Time.fixedDeltaTime);
            vehicle.SetThrottleInput(throttle);


            float steer_CrossTrack = steerPID_CrossTrack.Step(crossTrackLocal.x, Time.fixedDeltaTime);
            float steer_Projetcion_1 = steerPID_Projection_1.Step(p_1_Local.x, Time.fixedDeltaTime);
            float steer_Projetcion_2 = steerPID_Projection_2.Step(p_2_Local.x, Time.fixedDeltaTime);
            float steer_Projetcion_3 = steerPID_Projection_3.Step(p_3_Local.x, Time.fixedDeltaTime);

            float steer = (steer_CrossTrack * crossTrackWeight) + (steer_Projetcion_1 * projection_1_Weight) +
                (steer_Projetcion_2 * projection_2_Weight) + (steer_Projetcion_3 * projection_3_Weight);

            vehicle.SetSteerInput(steer);
        }


    }

}