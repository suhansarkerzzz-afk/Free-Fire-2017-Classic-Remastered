using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Single-finger virtual movement stick for Android and Editor testing.</summary>
public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform handle;
    [Range(0.1f, 1f)] public float handleRange = 0.42f;

    private RectTransform area;
    private Canvas parentCanvas;
    private Camera uiCamera;
    private int activePointerId = int.MinValue;

    private void Awake()
    {
        area = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = parentCanvas.worldCamera;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue && activePointerId != eventData.pointerId) return;
        activePointerId = eventData.pointerId;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activePointerId != eventData.pointerId || area == null || handle == null) return;

        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, uiCamera, out local)) return;

        Vector2 half = area.rect.size * 0.5f;
        Vector2 normalized = new Vector2(
            local.x / Mathf.Max(1f, half.x),
            local.y / Mathf.Max(1f, half.y));
        normalized = Vector2.ClampMagnitude(normalized, 1f);

        Vector2 travel = area.rect.size * (handleRange * 0.5f);
        handle.anchoredPosition = new Vector2(normalized.x * travel.x, normalized.y * travel.y);
        PlayerMovement.SetMobileMove(normalized);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activePointerId != eventData.pointerId) return;
        ResetStick();
    }

    private void OnDisable() { ResetStick(); }

    private void ResetStick()
    {
        activePointerId = int.MinValue;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        PlayerMovement.SetMobileMove(Vector2.zero);
    }
}
