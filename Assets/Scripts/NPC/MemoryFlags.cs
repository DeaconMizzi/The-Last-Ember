using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MemoryFlags
{
    public static void Set(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public static bool Get(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
