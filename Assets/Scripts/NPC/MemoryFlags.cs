using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MemoryFlags
{
    private static Dictionary<string, bool> flags = new Dictionary<string, bool>();

    public static void Set(string key)
    {
        flags[key] = true;
    }

    public static bool Get(string key)
    {
        return flags.ContainsKey(key) && flags[key];
    }
}
