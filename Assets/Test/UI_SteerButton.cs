using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//using UnityEngine.UIElements;
using UnityEngine.UI;

public class UI_SteerButton : Button, IDragHandler
{
    public Image steerRangeImage;
    public Image steerIndicatorImage;

    public float steerValue;


    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        //steerIndicatorImage.rectTransform.localPosition = Vector3.right * 50;

        Debug.Log("Down");

    }

    public void OnDrag(PointerEventData eventData)
    {
        steerValue = steerRangeImage.rectTransform.InverseTransformPoint(eventData.position).x;
        float halfWidth = steerRangeImage.rectTransform.sizeDelta.x / 2;
        steerValue = steerValue / halfWidth;
        steerValue = Mathf.Clamp(steerValue, -1, 1);

        steerIndicatorImage.rectTransform.localPosition = new Vector3(steerValue * halfWidth, 0, 1);
    }

    

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        steerIndicatorImage.rectTransform.localPosition = Vector3.right * 0;

        Debug.Log("Up");

    }

    
    void Start()
    {
        steerRangeImage = transform.Find("SteerRangeImage").GetComponent<Image>();
        steerIndicatorImage = steerRangeImage.transform.Find("SteerIndicatorImage").GetComponent<Image>();
    }

    void Update()
    {
        
    }
}
