using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmberOrbiter : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 1.5f;
    public float speed = 1f;
    public float angleOffset = 0f;

    private float angle = 0f;
    private bool isOrbiting = false;

    void Update()
    {
        if (!isOrbiting) return;
        Debug.Log("Orbiting: " + gameObject.name);
        angle += speed * Time.deltaTime;

        float x = Mathf.Cos(angle + angleOffset) * radius;
        float y = Mathf.Sin(angle + angleOffset) * radius;

        transform.position = centerPoint.position + new Vector3(x, y, 0f);
    }

   public void StartOrbit()
    {
        if (centerPoint == null)
        {
            Debug.LogWarning($" {gameObject.name} has no centerPoint assigned!");
            return;
        }

        // Calculate the angle offset based on current position relative to center
        Vector2 offset = transform.position - centerPoint.position;
        angleOffset = Mathf.Atan2(offset.y, offset.x); // Radians

        // Set radius based on current distance
        radius = offset.magnitude;

        isOrbiting = true;
        angle = 0f; // Reset orbit angle to start from offset
    }

    public void StopOrbit()
    {
        isOrbiting = false;
    }
}
