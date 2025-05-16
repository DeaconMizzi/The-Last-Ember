using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmber", menuName = "Ember System/Ember")]
public class EmberData : ScriptableObject
{
    public string emberName;
    public string description;
    public Sprite icon;
    public AbilityType abilityGranted;
    public GameObject worldEffectPrefab;
    public WorldShiftType worldShift;

    public enum AbilityType
    {
        None,
        Bloomstep,
        FlameGuard,
        CommandPulse
    }

    public enum WorldShiftType
    {
        Default,
        Overgrown,
        Seeded,
        Rewritten,
        Extinguished,
        Reforged
    }
}
