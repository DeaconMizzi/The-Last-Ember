using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOvergrowthManager : MonoBehaviour
{
    [Header("Tilemap Roots")]
    public GameObject wildernessGrid;
    public GameObject overgrownGrid;
    public GameObject natureParticles; // <-- this is now the parent "Particles" object

    private bool isOvergrown = false;

    public void ActivateOvergrowth()
    {
        // 1. Trigger camera shake — BEFORE the ember floats
        CameraShaker shaker = GameObject.Find("CmCam")?.GetComponent<CameraShaker>();
        if (shaker != null)
        {
            shaker.Shake(7f, 0.5f);
        }
        else
        {
            Debug.LogWarning("CameraShaker not found on CmCam.");
        }

        if (isOvergrown) return;

        if (wildernessGrid != null) wildernessGrid.SetActive(false);
        if (natureParticles != null)
             natureParticles.SetActive(true);
        if (overgrownGrid != null) overgrownGrid.SetActive(true);

        isOvergrown = true;
        Debug.Log("🌿 Overgrowth activated.");
        
    }
}