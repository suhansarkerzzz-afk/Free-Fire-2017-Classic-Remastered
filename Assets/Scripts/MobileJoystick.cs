using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform handle;
    public float radius = 65f;
    private RectTransform area;

    private void Awake() { area = GetComponent<RectTransform>(); }

    public void OnPointerDown(PointerEventData eventData) { OnDrag(eventData); }

    public void OnDrag(PointerEventData eventData)
    {
        if (area == null) area = GetComponent<RectTransform>();
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out local)) return;
        Vector2 value = Vector2.ClampMagnitude(local / Mathf.Max(1f, radius), 1f);
        if (handle != null) handle.anchoredPosition = value * radius;
        PlayerMovement.SetMobileMove(value);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayerMovement.SetMobileMove(Vector2.zero);
        if (handle != null) handle.anchoredPosition = Vector2.zero;
    }
}