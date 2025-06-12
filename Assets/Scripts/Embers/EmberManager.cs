using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmberManager : MonoBehaviour
{
    public static EmberManager Instance;

    public List<EmberData> claimedEmbers = new List<EmberData>();
    public GameObject player;
    public static bool LeftVerdantEmber = false;

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
        if (ember != null && !claimedEmbers.Contains(ember))
        {
            claimedEmbers.Add(ember);
            ApplyAbility(ember.abilityGranted);

            if (ember.worldEffectPrefab != null && player != null)
            {
                Instantiate(ember.worldEffectPrefab, player.transform.position, Quaternion.identity);
            }

            // If the player TAKES the Verdant Ember, ensure flag is false
            if (ember.emberID == EmberData.EmberID.Verdant)
                LeftVerdantEmber = false;
        }
        else
        {
            // If ember is null but shift is Overgrown, assume it was LEFT
            if (shift == EmberData.WorldShiftType.Overgrown)
                LeftVerdantEmber = true;
        }

        ApplyWorldChange(shift);
    }

    /// Applies the ability granted by the ember to the player.
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

    /// Applies environmental changes associated with the ember's world shift type.
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
    }
}
