using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundry;
    [SerializeField] string areaName;
    [SerializeField] Direction direction;
    [SerializeField] AreaNamePopup areaNamePopup; // Reference to the popup script

    enum Direction { Up, Down, Left, Right }

    private CinemachineConfiner confiner;

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();

        // Optional: auto-find the popup if not assigned in inspector
        if (areaNamePopup == null)
            areaNamePopup = FindObjectOfType<AreaNamePopup>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);

            if (areaNamePopup != null && !string.IsNullOrEmpty(areaName))
            {
                areaNamePopup.ShowAreaName(areaName);
            }

            MapController_Manual.Instance?.HighlightArea(mapBoundry.name);
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up: newPos.y += 2; break;
            case Direction.Down: newPos.y -= 2; break;
            case Direction.Left: newPos.x -= 2; break;
            case Direction.Right: newPos.x += 2; break;
        }
        player.transform.position = newPos;
    }
}
