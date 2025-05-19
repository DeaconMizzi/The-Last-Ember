using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalSceneVisualManager : MonoBehaviour
{
    public GameObject baseTilemap;
    public GameObject overgrownTilemap;

    void Start()
    {
        if (MemoryFlags.Get("LEAVE_VERDANT_EMBER"))
        {
            ShowOvergrown();
            Debug.Log("FinalScene: LEAVE_VERDANT_EMBER = " + MemoryFlags.Get("LEAVE_VERDANT_EMBER"));
        }
        else
        {
            ShowBase();
        }
    }

    void ShowBase()
    {
        baseTilemap.SetActive(true);
        overgrownTilemap.SetActive(false);
    }

    void ShowOvergrown()
    {
        overgrownTilemap.SetActive(true);
        baseTilemap.SetActive(false);
    }
}
