using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;

    [Header("Music Settings")]
    [Range(0f, 1f)]
    public float targetVolume = 1f; // Master music volume
    public float fadeDuration = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.volume = targetVolume; // Start at default volume
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.volume = targetVolume;
        audioSource.Play();
    }

    public void FadeOutMusic(float duration = -1f)
    {
        if (duration < 0f) duration = fadeDuration;
        StopAllCoroutines();
        StartCoroutine(FadeMusic(0f, duration));
    }

    public void FadeInMusic(float duration = -1f)
    {
        if (duration < 0f) duration = fadeDuration;
        StopAllCoroutines();
        StartCoroutine(FadeMusic(targetVolume, duration));
    }

    private IEnumerator FadeMusic(float target, float duration)
    {
        float currentTime = 0f;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, target, currentTime / duration);
            yield return null;
        }

        audioSource.volume = target;
    }

    public void SetVolume(float newVolume)
    {
        targetVolume = Mathf.Clamp01(newVolume);
        audioSource.volume = targetVolume;
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
