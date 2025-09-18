using UnityEngine;
using UnityEngine.UI;


public class Setting : MonoBehaviour
{
    [Header("Настройки звука")]
    public Slider music_slider;
    public Slider sound_slider;
    public AudioSource[] music;
    public AudioSource[] sound;

    private void Start()
    {
        // Инициализация сохраненных значений
        float savedVolumeM = PlayerPrefs.GetFloat("Music", 1f);
        float savedVolumeS = PlayerPrefs.GetFloat("Sound", 1f);

        // Установка начальных значений
        SetVolumeM(savedVolumeM);
        SetVolumeS(savedVolumeS);

        // Настройка слайдеров
        if (music_slider != null)
        {
            music_slider.value = savedVolumeM;
            music_slider.onValueChanged.AddListener(volume => {
                SetVolumeM(volume);
                PlayerPrefs.SetFloat("Music", volume); // Сохраняем сразу
            });
        }

        if (sound_slider != null)
        {
            sound_slider.value = savedVolumeS;
            sound_slider.onValueChanged.AddListener(volume => {
                SetVolumeS(volume);
                PlayerPrefs.SetFloat("Sound", volume); // Сохраняем сразу
            });
        }
    }

    public void SetVolumeM(float volume)
    {
        if (music == null) return;

        foreach (var audioSource in music)
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }

    public void SetVolumeS(float volume)
    {
        if (sound == null) return;

        foreach (var audioSource in sound)
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }

    // Сохранение при выходе (опционально)
    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}