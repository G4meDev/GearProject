using UnityEngine;

public class Controller_PID : MonoBehaviour
{
    public float pFactor, iFactor, dFactor;

    private float _integral;
    private float _lastError;
    public void Init(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }
    public float Step(float target, float current, float deltatime)
    {
        float error = target - current;
        _integral += error * deltatime;
        float derivative = (error - _lastError) / deltatime;
        _lastError = error;
        return error * pFactor + _integral * iFactor + derivative * dFactor;
    }
    public void LimitIntegral(float value)
    {
        if (_integral >= value)
        {
            _integral = value;
        }
        if (_integral <= -value)
        {
            _integral = -value;
        }
    }
}