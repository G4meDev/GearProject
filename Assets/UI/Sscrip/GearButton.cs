using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GearButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool bDown = false;

    public float value;

    public float raiseRate = 3;
    public float fallRate = 3;

    void FixedUpdate()
    {
        value = Mathf.Clamp01(value + (bDown ? raiseRate : -fallRate) * Time.fixedDeltaTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        bDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        bDown = false;
    }
}
