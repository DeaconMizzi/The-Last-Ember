using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapController_Manual : MonoBehaviour
{
   public static MapController_Manual Instance {get; set;}

   public GameObject mapParent;
   List<Image> mapImages;
   public Color highlightColour = Color.yellow;
   public Color dimmedColour = new Color(1f, 1f, 1f, 0.5f);

   public RectTransform playerIconTransform;

   private HashSet<string> discoveredAreas = new HashSet<string>();
   public string startingAreaName = "Houses";


    private void Start()
    {
        // Refresh list in case any were added dynamically
        mapImages = mapParent.GetComponentsInChildren<Image>().ToList();

        // Hide all initially
        foreach (Image area in mapImages)
        {
            area.enabled = false;
        }

        // Hide the player icon until an area is shown
        playerIconTransform.gameObject.SetActive(false);

        // Immediately highlight the starting area
        HighlightArea(startingAreaName);
    }

   private void Awake()
   {
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
    }
    else
    {
        Instance = this;
    
    }
    mapImages = mapParent.GetComponentsInChildren<Image>().ToList();
   }

  public void HighlightArea(string areaName)
   {
    mapImages = mapParent.GetComponentsInChildren<Image>().ToList();
    discoveredAreas.Add(areaName);

    foreach (Image area in mapImages)
    {
        if (discoveredAreas.Contains(area.name))
        {
            area.enabled = true;
            area.color = dimmedColour;
        }
        else
        {
            area.enabled = false;
        }
    }

    Image currentArea = mapImages.Find(x => x.name == areaName);
    if (currentArea != null)
    {
        currentArea.color = highlightColour;
        playerIconTransform.position = currentArea.GetComponent<RectTransform>().position;
        playerIconTransform.gameObject.SetActive(true);
    }
    else
    {
        Debug.LogWarning("Area not found: " + areaName);
    }
   }

}
