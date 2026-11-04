using UnityEngine;

public class TouchCameraLook : MonoBehaviour
{
    private FollowCamera follow;

    private void Awake() { follow = GetComponent<FollowCamera>(); }

    private void Update()
    {
        if (follow == null) return;
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width * 0.42f)
            follow.AddLookDelta(new Vector2(Input.GetAxis("Mouse X") * 18f, Input.GetAxis("Mouse Y") * 18f));
#endif
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.position.x < Screen.width * 0.42f) continue;
            if (touch.phase == TouchPhase.Moved)
                follow.AddLookDelta(touch.deltaPosition);
        }
    }
}