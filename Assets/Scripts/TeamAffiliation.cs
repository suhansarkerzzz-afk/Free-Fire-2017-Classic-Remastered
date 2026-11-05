using UnityEngine;

// Lightweight squad identity used by bot target selection and later LAN team sync.
public class TeamAffiliation : MonoBehaviour
{
    [Range(0, 15)] public int teamId;
    public string teamName = "SOLO";
    public bool IsFriendly(TeamAffiliation other)
    {
        return other != null && teamId == other.teamId;
    }
    public static void Assign(GameObject actor, int id)
    {
        if (actor == null) return;
        TeamAffiliation affiliation = actor.GetComponent<TeamAffiliation>();
        if (affiliation == null) affiliation = actor.AddComponent<TeamAffiliation>();
        affiliation.teamId = Mathf.Clamp(id, 0, 15);
        affiliation.teamName = id == 0 ? "PLAYER SQUAD" : "SQUAD " + id;
    }
}
