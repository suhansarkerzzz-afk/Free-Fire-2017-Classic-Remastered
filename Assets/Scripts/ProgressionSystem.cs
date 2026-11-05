using UnityEngine;
using UnityEngine.UI;

// Offline rank and mission progression saved locally on the device.
public class ProgressionSystem : MonoBehaviour
{
    public int TotalKills { get; private set; }
    public int RankPoints { get; private set; }
    private Text label;
    private void Start()
    {
        TotalKills = PlayerPrefs.GetInt("Classic_TotalKills", 0);
        RankPoints = PlayerPrefs.GetInt("Classic_RankPoints", 0);
        BattleRoyaleMatchManager match = FindObjectOfType<BattleRoyaleMatchManager>();
        if (match != null) match.OnElimination += RegisterElimination;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            GameObject obj = new GameObject("MissionProgress", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(canvas.transform, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.84f);
            rect.anchorMax = new Vector2(0.5f, 0.84f);
            rect.sizeDelta = new Vector2(420f, 30f);
            label = obj.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 16; label.alignment = TextAnchor.MiddleCenter; label.color = Color.yellow;
            Refresh();
        }
    }
    public void RegisterElimination()
    {
        TotalKills++;
        RankPoints += 10;
        PlayerPrefs.SetInt("Classic_TotalKills", TotalKills);
        PlayerPrefs.SetInt("Classic_RankPoints", RankPoints);
        PlayerPrefs.Save();
        Refresh();
    }
    public void RegisterMatchResult(bool won)
    {
        RankPoints += won ? 100 : 10;
        PlayerPrefs.SetInt("Classic_RankPoints", RankPoints);
        PlayerPrefs.Save();
        Refresh();
    }
    private void Refresh()
    {
        if (label != null) label.text = "RANK POINTS: " + RankPoints + "  |  TOTAL KILLS: " + TotalKills + "  |  MISSION: 10 KILLS";
    }
}
