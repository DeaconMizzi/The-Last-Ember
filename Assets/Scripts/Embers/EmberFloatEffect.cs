using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmberFloatEffect : MonoBehaviour
{
    public float floatHeight = 1.5f;
    public float floatDuration = 1f;
    public float fadeDuration = 1f;

    private SpriteRenderer sr;
    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
        targetPos = startPos + Vector3.up * floatHeight;
    }

    public void PlayEffect()
    {
        StartCoroutine(FloatAndFade());
    }

    private System.Collections.IEnumerator FloatAndFade()
    {
        float t = 0f;

        // FLOAT UP
        while (t < floatDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t / floatDuration);
            yield return null;
        }

        // FADE OUT
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject); // Or setActive(false) if you prefer
    }
}
