using UnityEngine;
using UnityEngine.UI;

// Offline 50-participant match director: one human player and configurable AI opponents.
public class BattleRoyaleMatchManager : MonoBehaviour
{
    [Range(1,49)] public int botCount = 49;
    public float spawnRadius = 48f;
    public Transform player;
    private int aliveBots;
    private int eliminations;
    private Text statusLabel;
    private bool matchEnded;

    public int AliveParticipants { get { return (player != null && player.GetComponent<HealthSystem>() != null && !player.GetComponent<HealthSystem>().IsDead ? 1 : 0) + aliveBots; } }
    public int Eliminations { get { return eliminations; } }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        aliveBots = botCount;
        CreateStatusHud();
        HealthSystem playerHealth = player.GetComponent<HealthSystem>();
        if (playerHealth != null) playerHealth.OnDied += OnPlayerDied;

        for (int i = 0; i < botCount; i++)
        {
            Vector2 point = Random.insideUnitCircle;
            if (point.sqrMagnitude < 0.01f) point = Vector2.right;
            point = point.normalized * Random.Range(spawnRadius * 0.45f, spawnRadius);
            GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = "BattleBot_" + (i + 1).ToString("00");
            bot.transform.position = new Vector3(point.x, 1f, point.y);
            bot.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            bot.GetComponent<Renderer>().material.color = new Color(0.72f, 0.18f, 0.13f);
            BasicBotAI ai = bot.AddComponent<BasicBotAI>();
            ai.target = player;
            ai.moveSpeed = Random.Range(1.7f, 2.35f);
            HealthSystem health = bot.AddComponent<HealthSystem>();
            health.OnDied += () => OnBotDied(bot);
            BotCombat combat = bot.AddComponent<BotCombat>();
            combat.target = player;
        }

        gameObject.AddComponent<SafeZoneController>().target = player;
        SpawnLoot();
        RefreshHud();
    }

    private void CreateStatusHud()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        GameObject labelObject = new GameObject("BattleRoyaleStatus", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.96f);
        rect.anchorMax = new Vector2(0.5f, 0.96f);
        rect.sizeDelta = new Vector2(420f, 42f);
        statusLabel = labelObject.GetComponent<Text>();
        statusLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusLabel.fontSize = 22;
        statusLabel.alignment = TextAnchor.MiddleCenter;
        statusLabel.color = Color.white;
    }

    private void OnBotDied(GameObject bot)
    {
        if (bot == null) return;
        aliveBots = Mathf.Max(0, aliveBots - 1);
        eliminations++;
        bot.SetActive(false);
        RefreshHud();
        if (aliveBots <= 0 && !matchEnded) EndMatch(true);
    }

    private void OnPlayerDied()
    {
        if (!matchEnded) EndMatch(false);
    }

    private void EndMatch(bool won)
    {
        matchEnded = true;
        if (statusLabel != null)
        {
            statusLabel.text = won ? "BOOYAH!  |  ELIMINATIONS: " + eliminations : "ELIMINATED  |  ELIMINATIONS: " + eliminations;
            statusLabel.color = won ? new Color(1f, 0.78f, 0.12f) : Color.red;
        }
        Time.timeScale = 0.0f;
    }

    private void RefreshHud()
    {
        if (statusLabel != null) statusLabel.text = "ALIVE: " + AliveParticipants + "  |  KILLS: " + eliminations;
    }

    private void SpawnLoot()
    {
        for (int i = 0; i < 18; i++)
        {
            Vector2 p = Random.insideUnitCircle * 38f;
            GameObject loot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            loot.name = i % 3 == 0 ? "MedkitPickup" : "AmmoPickup";
            loot.transform.position = new Vector3(p.x, 0.55f, p.y);
            loot.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            loot.GetComponent<Renderer>().material.color = i % 3 == 0 ? new Color(0.15f, 0.85f, 0.3f) : new Color(0.95f, 0.72f, 0.12f);
            loot.AddComponent<LootPickup>().kind = i % 3 == 0 ? LootPickup.LootKind.Medkit : LootPickup.LootKind.Ammo;
        }
    }
}
