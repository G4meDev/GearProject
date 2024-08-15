using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpeedModifierData : ScriptableObject
{
    public SpeedModiferMode mode = SpeedModiferMode.addtive;
    public SpeedModiferType type = SpeedModiferType.boostpad;
    public float intensity = 10.0f;

    public float startup = 0.2f;
    public float duration = 2.0f;
    public float decay = 0.2f;
}