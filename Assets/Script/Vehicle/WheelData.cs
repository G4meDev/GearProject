using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WheelData : ScriptableObject
{
    public float WheelWidth = 0.32f;
    public float WheelRadius = 0.22f;

    public float MaxSuspensionRaise = 0.05f;
    public float MaxSuspensionLower = 0.1f;

    public float SuspensionStrength = 100.0f;
    public float SuspensionDamper = 15.0f;
}
