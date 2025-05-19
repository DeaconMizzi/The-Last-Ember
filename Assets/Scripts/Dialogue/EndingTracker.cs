using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTracker : MonoBehaviour
{
    public static EndingTracker Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Keeps the tracker alive between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetEmberScore()
    {
        int score = 0;

        if (MemoryFlags.Get("TAKE_VERDANT_EMBER")) score += 1;

        if (MemoryFlags.Get("TAKE_DOMINION_EMBER")) score += 1;
        if (MemoryFlags.Get("LEAVE_DOMINION_EMBER")) score -= 1;

        if (MemoryFlags.Get("TAKE_WRATH_EMBER")) score += 1;
        if (MemoryFlags.Get("LEAVE_WRATH_EMBER")) score -= 1;

        return score;
    }

    public string GetEndingID()
    {
        bool tookAll = MemoryFlags.Get("TAKE_VERDANT_EMBER") &&
                       MemoryFlags.Get("TAKE_DOMINION_EMBER") &&
                       MemoryFlags.Get("TAKE_WRATH_EMBER");

        bool leftAny = MemoryFlags.Get("LEAVE_VERDANT_EMBER") ||
                       MemoryFlags.Get("LEAVE_DOMINION_EMBER") ||
                       MemoryFlags.Get("LEAVE_WRATH_EMBER");

        if (tookAll && !leftAny)
            return "GOOD_ENDING";

        int score = GetEmberScore();

        if (score >= 1)
            return "NEUTRAL_ENDING";

        return "BAD_ENDING";
    }
}
