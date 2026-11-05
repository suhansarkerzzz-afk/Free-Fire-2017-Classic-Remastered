using UnityEngine;

// Simple local emotes using procedural pose offsets; no external animation assets.
public class EmoteController : MonoBehaviour
{
    private Transform model;
    private float emoteUntil;
    private int activeEmote;
    private void Start()
    {
        ResolveModel();
    }
    private void ResolveModel()
    {
        if (GetComponent<ClassicCharacterVisual>() != null)
            model = transform.Find("ClassicCharacterModel");
    }
    public void PlayEmote(int emote)
    {
        activeEmote = Mathf.Clamp(emote, 0, 2);
        emoteUntil = Time.time + 2.8f;
    }
    private void Update()
    {
        // Component Start order is not guaranteed; retry after the visual component creates its model.
        if (model == null) ResolveModel();
        if (model == null) return;
        if (Time.time > emoteUntil) { model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.identity, Time.deltaTime * 5f); return; }
        float phase = (Time.time * 8f);
        if (activeEmote == 0) model.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * 8f);
        else if (activeEmote == 1) model.localRotation = Quaternion.Euler(0f, Mathf.Sin(phase) * 22f, 0f);
        else model.localRotation = Quaternion.Euler(Mathf.Sin(phase) * 8f, 0f, Mathf.Sin(phase * 0.5f) * 10f);
    }
}
