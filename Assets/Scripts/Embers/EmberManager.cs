using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmberManager : MonoBehaviour
{
    public static EmberManager Instance;

    public EmberData currentEmber;
    public GameObject player;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetEmber(EmberData ember, EmberData.WorldShiftType shift)
    {
        currentEmber = ember;
        ApplyAbility(ember.abilityGranted);
        ApplyWorldChange(shift);
    }

    void ApplyAbility(EmberData.AbilityType ability)
    {
        var abilities = player.GetComponent<PlayerAbilities>();
        if (abilities == null) return;

        switch (ability)
        {
            case EmberData.AbilityType.Bloomstep:
                abilities.EnableBloomstep();
                break;
            case EmberData.AbilityType.FlameGuard:
                abilities.EnableFlameGuard();
                break;
            case EmberData.AbilityType.CommandPulse:
                abilities.EnableCommandPulse();
                break;
        }
    }

    void ApplyWorldChange(EmberData.WorldShiftType shift)
    {
        // Placeholder: add your visual/environment logic here.
        switch (shift)
        {
            case EmberData.WorldShiftType.Overgrown:
                Debug.Log("World becomes overgrown.");
                break;
            case EmberData.WorldShiftType.Seeded:
                Debug.Log("World gently regrows.");
                break;
            case EmberData.WorldShiftType.Extinguished:
                Debug.Log("The land cools and settles.");
                break;
            case EmberData.WorldShiftType.Reforged:
                Debug.Log("The world stabilizes with strength.");
                break;
        }

        if (currentEmber.worldEffectPrefab != null)
        {
            Instantiate(currentEmber.worldEffectPrefab, player.transform.position, Quaternion.identity);
        }
    }
}