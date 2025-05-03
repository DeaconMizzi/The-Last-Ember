using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AreaNamePopup : MonoBehaviour
{
    public TextMeshProUGUI areaNameText;
    public float fadeDuration = 1f;
    public float displayDuration = 2f;

    private Coroutine fadeRoutine;

    public void ShowAreaName(string areaName)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(areaName));
    }

    private IEnumerator FadeRoutine(string name)
    {
        areaNameText.text = name;
        Color baseColor = areaNameText.color;

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float a = t / fadeDuration;
            areaNameText.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
        areaNameText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float a = 1f - (t / fadeDuration);
            areaNameText.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
        areaNameText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
    }
}
