using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum SpeedModiferMode
{
    addtive,
    baseSpeedMultiply
}

public enum SpeedModiferType
{
    boostpad,
    trickboost
}

public class SC_SpeedModifier : MonoBehaviour
{
    public SpeedModifierData speedModifierData;

    [HideInInspector]
    public float value;

    [HideInInspector]
    public float lifeTime = 0.0f;

    [HideInInspector]
    public bool alive = false;

    void Start()
    {
        
    }

    public void Register(SpeedModifierData data)
    {
        alive = true;
        lifeTime = 0.0f;

        speedModifierData = data;
    }

    void FixedUpdate()
    {
        if (alive)
        {
            lifeTime += Time.fixedDeltaTime;

            float alpha;

            if (lifeTime < speedModifierData.startup)
            {
                alpha = math.remap(0, speedModifierData.startup, 0, 1, lifeTime);
            }

            else if (lifeTime < speedModifierData.startup + speedModifierData.duration)
            {
                alpha = 1;
            }

            else
            {
                float a = speedModifierData.startup + speedModifierData.duration;
                alpha = math.remap(a, a + speedModifierData.decay, 1, 0, lifeTime);

                if (alpha < 0)
                {
                    // destroy
                    alive = false;
                }
            }

            alpha = Mathf.Clamp01(alpha);

            if (speedModifierData.mode == SpeedModiferMode.addtive)
            {
                value = alpha * speedModifierData.intensity;
            }

            else if (speedModifierData.mode == SpeedModiferMode.baseSpeedMultiply)
            {
                value = Mathf.Lerp(1, speedModifierData.intensity, alpha);
            }


        }
    }
}
