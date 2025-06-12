using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObjectiveCompleter : MonoBehaviour
{
   void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("TAKE_VERDANT_EMBER triggered");
            MemoryFlags.Set("TAKE_VERDANT_EMBER");
            EmberManager.Instance.SetEmber(GameStateController.Instance.verdantEmberData, EmberData.WorldShiftType.Seeded);
            QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_VERDANT", 1);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("LEAVE_VERDANT_EMBER triggered");
            MemoryFlags.Set("LEAVE_VERDANT_EMBER");
            EmberManager.Instance.SetEmber(null, EmberData.WorldShiftType.Overgrown);
            QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_VERDANT", 1);
            var gridManager = FindObjectOfType<GridOvergrowthManager>();
            if (gridManager != null)
            {
                gridManager.ActivateOvergrowth();
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("TAKE_DOMINION_EMBER triggered");
            MemoryFlags.Set("TAKE_DOMINION_EMBER");
            EmberManager.Instance.SetEmber(GameStateController.Instance.dominionEmberData, EmberData.WorldShiftType.Reforged);
            QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_DOMINION", 1);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("LEAVE_DOMINION_EMBER triggered");
            MemoryFlags.Set("LEAVE_DOMINION_EMBER");
            EmberManager.Instance.SetEmber(null, EmberData.WorldShiftType.Reforged);
            QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_DOMINION", 1);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("TAKE_WRATH_EMBER triggered");
            MemoryFlags.Set("TAKE_WRATH_EMBER");
            EmberManager.Instance.SetEmber(GameStateController.Instance.wrathEmberData, EmberData.WorldShiftType.Reforged);
            QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_WRATH", 1);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("LEAVE_WRATH_EMBER triggered");
            MemoryFlags.Set("LEAVE_WRATH_EMBER");
            EmberManager.Instance.SetEmber(null, EmberData.WorldShiftType.Extinguished);
            QuestManager.Instance?.IncrementObjective("COLLECT_EMBER", "EMBER_WRATH", 1);
        }
    }
}