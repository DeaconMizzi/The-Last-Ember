using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTracker : MonoBehaviour
{
    public static EndingTracker Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        int score = GetEmberScore();

        if (score >= 3)
            return "GOOD_ENDING";
        else if (score >= 1)
            return "NEUTRAL_ENDING";
        else
            return "BAD_ENDING";
    }
}