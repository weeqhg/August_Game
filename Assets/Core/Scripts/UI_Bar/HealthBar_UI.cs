using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar_UI : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider;
    private PlayerHealth playerHealth;


    private void Start()
    {
        healthBarSlider.interactable = false;
    }

    public void Initialize(PlayerHealth newPlayerHealth)
    {
        if (newPlayerHealth == null)
        {
            Debug.LogError("PlayerHealth is null!", this);
            return;
        }

        playerHealth = newPlayerHealth;

        // Отписываемся от предыдущих событий (если были)
        if (playerHealth.OnHealthChanged != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(RefreshUI);
        }

        playerHealth.OnHealthChanged.AddListener(RefreshUI);
    }

    public void RefreshUI(float newCurrentHealth, float newMaxHealth)
    {
        if (playerHealth == null || healthBarSlider == null)
            return;

        healthBarSlider.maxValue = newMaxHealth;
        healthBarSlider.value = newCurrentHealth;

    }
}
