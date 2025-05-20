using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource musicSource;

    public float fadeDuration = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicSource = GetComponent<AudioSource>();
    }

    public void FadeOutMusic()
    {
        StartCoroutine(FadeMusicRoutine(1f, 0f));
    }

    public void FadeInMusic()
    {
        StartCoroutine(FadeMusicRoutine(0f, 1f));
    }

    IEnumerator FadeMusicRoutine(float fromVolume, float toVolume)
    {
        float timer = 0f;
        musicSource.volume = fromVolume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(fromVolume, toVolume, timer / fadeDuration);
            yield return null;
        }

        musicSource.volume = toVolume;
    }
}