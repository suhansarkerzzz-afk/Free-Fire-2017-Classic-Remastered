using System;
using UnityEngine;

// Local room-code utility for the planned LAN room flow.
// This creates shareable codes only; network transport is implemented separately.
public static class LocalRoomCode
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public static string CreateCode()
    {
        char[] chars = new char[6];
        for (int i = 0; i < chars.Length; i++) chars[i] = Alphabet[UnityEngine.Random.Range(0, Alphabet.Length)];
        return new string(chars);
    }
    public static bool IsValid(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 6) return false;
        foreach (char c in code.ToUpperInvariant())
            if (Alphabet.IndexOf(c) < 0) return false;
        return true;
    }
}
