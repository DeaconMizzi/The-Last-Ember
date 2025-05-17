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

    /// <summary>
    /// Assigns an ember and triggers its effects. If the ember is null, only the world shift will occur.
    /// </summary>
    public void SetEmber(EmberData ember, EmberData.WorldShiftType shift)
    {
        currentEmber = ember;

        if (ember != null)
            ApplyAbility(ember.abilityGranted);

        ApplyWorldChange(shift);
    }

    /// <summary>
    /// Applies the ability granted by the ember to the player.
    /// </summary>
    void ApplyAbility(EmberData.AbilityType ability)
    {
        if (player == null) return;

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

    /// <summary>
    /// Applies environmental changes associated with the ember's world shift type.
    /// </summary>
    void ApplyWorldChange(EmberData.WorldShiftType shift)
    {
        switch (shift)
        {
            case EmberData.WorldShiftType.Overgrown:
                Debug.Log("🌿 World becomes overgrown.");
                break;
            case EmberData.WorldShiftType.Seeded:
                Debug.Log("🌱 World gently regrows.");
                break;
            case EmberData.WorldShiftType.Extinguished:
                Debug.Log("🕯️ The land cools and settles.");
                break;
            case EmberData.WorldShiftType.Reforged:
                Debug.Log("🛠️ The world stabilizes with strength.");
                break;
        }

        if (currentEmber != null && currentEmber.worldEffectPrefab != null)
        {
            Instantiate(currentEmber.worldEffectPrefab, player.transform.position, Quaternion.identity);
        }
    }
}
