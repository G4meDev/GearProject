using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GearButton : Button
{
    public bool pressed = false;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        pressed = true;
    }


    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        pressed = false;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
