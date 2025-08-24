using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraShakeController : MonoBehaviour
{
    [Header("Cinemachine Shake Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float defaultAmplitude = 1.2f;
    [SerializeField] private float defaultFrequency = 2.0f;
    [SerializeField] private float defaultDuration = 0.3f;

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine currentShakeCoroutine;

    private void Awake()
    {
        InitializeCinemachineNoise();
        GameManager.Instance.Register(this);
    }

    private void InitializeCinemachineNoise()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera not found!");
                return;
            }
        }

        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found!");
        }
        ShakeCamera();
    }

    /// <summary>
    /// Запуск тряски камеры
    /// </summary>
    public void ShakeCamera(float amplitude, float frequency, float duration)
    {
        if (noise == null) return;

        // Останавливаем предыдущую тряску
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    /// <summary>
    /// Запуск тряски с настройками по умолчанию
    /// </summary>
    public void ShakeCamera()
    {
        ShakeCamera(defaultAmplitude, defaultFrequency, defaultDuration);
    }

    /// <summary>
    /// Запуск тряски с кастомной амплитудой
    /// </summary>
    public void ShakeCamera(float amplitude)
    {
        ShakeCamera(amplitude, defaultFrequency, defaultDuration);
    }

    /// <summary>
    /// Запуск тряски с кастомной амплитудой и длительностью
    /// </summary>
    public void ShakeCamera(float amplitude, float duration)
    {
        ShakeCamera(amplitude, defaultFrequency, duration);
    }

    private IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        // Устанавливаем параметры шума
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;

        // Ждем указанное время
        yield return new WaitForSeconds(duration);

        // Плавно убираем тряску
        float fadeOutTime = 0.1f;
        float elapsed = 0f;
        float startAmplitude = noise.m_AmplitudeGain;
        float startFrequency = noise.m_FrequencyGain;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutTime;

            noise.m_AmplitudeGain = Mathf.Lerp(startAmplitude, 0f, t);
            noise.m_FrequencyGain = Mathf.Lerp(startFrequency, 0f, t);

            yield return null;
        }

        // Гарантированно выключаем тряску
        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
        currentShakeCoroutine = null;
    }

    /// <summary>
    /// Немедленная остановка тряски
    /// </summary>
    public void StopShakeImmediately()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }

        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    /// <summary>
    /// Плавная тряска с затуханием
    /// </summary>
    public void ShakeCameraSmooth(float amplitude, float frequency, float duration, float fadeInTime = 0.1f)
    {
        if (noise == null) return;

        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = StartCoroutine(SmoothShakeRoutine(amplitude, frequency, duration, fadeInTime));
    }

    private IEnumerator SmoothShakeRoutine(float amplitude, float frequency, float duration, float fadeInTime)
    {
        // Плавное нарастание
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInTime;

            noise.m_AmplitudeGain = Mathf.Lerp(0f, amplitude, t);
            noise.m_FrequencyGain = Mathf.Lerp(0f, frequency, t);

            yield return null;
        }

        // Основная длительность
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
        yield return new WaitForSeconds(duration - fadeInTime * 2);

        // Плавное затухание
        elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInTime;

            noise.m_AmplitudeGain = Mathf.Lerp(amplitude, 0f, t);
            noise.m_FrequencyGain = Mathf.Lerp(frequency, 0f, t);

            yield return null;
        }

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
        currentShakeCoroutine = null;
    }

    /// <summary>
    /// Проверка, активна ли тряска
    /// </summary>
    public bool IsShaking()
    {
        return currentShakeCoroutine != null || (noise != null && noise.m_AmplitudeGain > 0f);
    }

    // Для отладки
    [ContextMenu("Test Shake")]
    private void TestShake()
    {
        ShakeCamera();
    }

    [ContextMenu("Test Strong Shake")]
    private void TestStrongShake()
    {
        ShakeCamera(2f, 2.5f, 0.5f);
    }

    private void OnDestroy()
    {
        StopShakeImmediately();
    }
}