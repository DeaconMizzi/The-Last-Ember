using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashGhost : MonoBehaviour
{
    public float fadeDuration = 0.5f;

    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        timer = fadeDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, 1f - (timer / fadeDuration));
            sr.color = c;
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
