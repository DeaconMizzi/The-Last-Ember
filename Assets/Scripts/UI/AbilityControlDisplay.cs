using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityControlDisplay : MonoBehaviour
{
    public GameObject bloomstepControl;
    public GameObject flameguardControl;
    public GameObject commandpulseControl;

    void Start()
    {
        // Hide all by default
        bloomstepControl.SetActive(false);
        flameguardControl.SetActive(false);
        commandpulseControl.SetActive(false);

        // Show based on unlocked flags
        if (MemoryFlags.Get("TAKE_VERDANT_EMBER"))
            bloomstepControl.SetActive(true);

        if (MemoryFlags.Get("TAKE_WRATH_EMBER"))
            flameguardControl.SetActive(true);

        if (MemoryFlags.Get("TAKE_DOMINION_EMBER"))
            commandpulseControl.SetActive(true);
    }
}
