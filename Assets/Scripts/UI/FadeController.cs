using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    public static FadeController Instance;

    void Awake()
    {
        Instance = this;
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        var c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }

    public IEnumerator FadeOutIn(System.Action onMidpoint = null)
    {
        yield return Fade(1f); // Fade to black
        onMidpoint?.Invoke();
        yield return Fade(0f); // Fade back in
    }

    IEnumerator Fade(float targetAlpha)
    {
        float start = fadeImage.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start, targetAlpha, time / fadeDuration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }
}
