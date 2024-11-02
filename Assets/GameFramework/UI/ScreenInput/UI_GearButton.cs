using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GearButton : Button
{
    public bool pressed = false;
    public bool entered = false;

    public delegate void OnReleasedButton();
    public OnReleasedButton OnRelease;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        pressed = true;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        entered = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        entered = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        OnRelease?.Invoke();

        pressed = false;
        entered = false;
    }

    public void OnActive()
    {
        image.material.SetFloat("_pressed", 1);

        pressed = true;
    }

    public void OnDeactive()
    {
        image.material.SetFloat("_pressed", 0);

        pressed = false;
        entered = false;
    }
}
