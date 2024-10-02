using UnityEngine;

public class AI_Position_Params
{
    public float optimalPathChance;
    public float minSpeed;
    public float maxSpeed;
    public float rubberBadingDist;

    public AI_Position_Params(float inOptimalPathChance, float inMinSpeed, float inMaxSpeed, float inRubberBadingDist)
    {
        optimalPathChance = inOptimalPathChance;
        minSpeed = inMinSpeed;
        maxSpeed = inMaxSpeed;
        rubberBadingDist = inRubberBadingDist;
    }
}

public class AI_Params
{
    // universal params
    public static float rbOptimalPathChanceIncrease = 0.2f;
    public static float rbSpeedIncrease = 10.0f;

    public static float maxDriftableTurnAlpha = 0.4f;
    public static float driftHaltDistanceToRoadEdge = 5.0f;
    public static float maxDriftHaltAngleAlpha = 0.095f;


    // --------------------------------------------------------
    // -------------------------------      optimalPathChance   , minSpeed  , maxSpeed  , rubberBanddingDist
    static AI_Position_Params pos_1 = new(0.9f                  , 40        , 43        , 1);
    static AI_Position_Params pos_2 = new(0.8f                  , 38        , 41        , 1.5f);
    static AI_Position_Params pos_3 = new(0.7f                  , 36        , 39        , 2.5f);
    static AI_Position_Params pos_4 = new(0.55f                 , 34        , 37        , 3.0f);
    static AI_Position_Params pos_5 = new(0.5f                  , 32        , 35        , 3.5f);
    static AI_Position_Params pos_6 = new(0.4f                  , 30        , 33        , 4.5f);
    static AI_Position_Params pos_7 = new(0.35f                 , 28        , 31        , 5.0f);

    public static ref AI_Position_Params GetPositionParams(int pos)
    {
        switch (pos)
        {
            case 1: return ref pos_1;
            case 2: return ref pos_2;
            case 3: return ref pos_3;
            case 4: return ref pos_4;
            case 5: return ref pos_5;
            case 6: return ref pos_6;
            case 7: return ref pos_7;

            default: return ref pos_7;
        }
    }
}