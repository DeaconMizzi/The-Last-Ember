using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAndDestroy : MonoBehaviour
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
            c.a = Mathf.Clamp01(timer / fadeDuration);
            sr.color = c;
        }

        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}   