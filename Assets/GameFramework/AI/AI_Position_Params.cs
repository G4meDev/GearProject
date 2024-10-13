using UnityEngine;

public class AI_Position_Params
{
    public float minSpeed;
    public float maxSpeed;
    public float rubberBadingDist;

    public AI_Position_Params(float inMinSpeed, float inMaxSpeed, float inRubberBadingDist)
    {
        minSpeed = inMinSpeed;
        maxSpeed = inMaxSpeed;
        rubberBadingDist = inRubberBadingDist;
    }
}

public class AI_Params
{
    // universal params
    public static float rbSpeedIncrease = 10.0f;



    public static float projection_1_dist = 15;
    public static float projection_2_dist = 40;
    public static float projection_3_dist = 100;


    // --------------------------------------------------------
    // -------------------------------    minSpeed  , maxSpeed  , rubberBanddingDist
    static AI_Position_Params pos_1 = new(40        , 45        , 30);
    static AI_Position_Params pos_2 = new(38        , 41        , 50);
    static AI_Position_Params pos_3 = new(36        , 39        , 80);
    static AI_Position_Params pos_4 = new(34        , 37        , 90);
    static AI_Position_Params pos_5 = new(32        , 35        , 100);
    static AI_Position_Params pos_6 = new(30        , 33        , 130);
    static AI_Position_Params pos_7 = new(28        , 31        , 140);

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