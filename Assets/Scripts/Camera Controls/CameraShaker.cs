using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    private float shakeDuration;
    private float originalAmplitude;

    void Start()
    {
        var virtualCam = GetComponent<CinemachineVirtualCamera>();
        if (virtualCam != null)
        {
            noise = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                originalAmplitude = noise.m_AmplitudeGain;
            }
        }
    }

    public void Shake(float duration, float amplitude)
    {
        if (noise == null)
        {
            Debug.LogWarning("CameraShaker: No noise component found.");
            return;
        }

        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = 2f;
        shakeTimer = duration;
        shakeDuration = duration;
    }

    void Update()
    {
        if (noise == null || shakeTimer <= 0f) return;

        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0f)
        {
            noise.m_AmplitudeGain = originalAmplitude;
            noise.m_FrequencyGain = 0f;
        }
    }
}
