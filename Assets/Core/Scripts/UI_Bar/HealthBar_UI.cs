using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBar_UI : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private Slider fillImage;
    [SerializeField] private Image damageFlashImage;

    [Header("Animation Settings")]
    [SerializeField] private float damageAnimationDuration = 0.3f;
    [SerializeField] private float healAnimationDuration = 0.5f;
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Ease damageEase = Ease.OutQuad;
    [SerializeField] private Ease healEase = Ease.OutBack;

    private PlayerHealth playerHealth;
    private float _currentDisplayHealth;
    private Tween _healthTween;
    private Tween _flashTween;

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
        healthBarSlider.maxValue = playerHealth.GetMaxHealth();
        healthBarSlider.value = playerHealth.GetCurrentHealth();
        _currentDisplayHealth = playerHealth.GetCurrentHealth();
        //Debug.Log(playerHealth.GetMaxHealth());
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

        // Определяем, это урон или лечение
        bool isDamage = newCurrentHealth < _currentDisplayHealth;
        bool isHeal = newCurrentHealth > _currentDisplayHealth;

        // Останавливаем предыдущие анимации
        _healthTween?.Kill();
        _flashTween?.Kill();

        // Анимация вспышки урона/лечения
        if (damageFlashImage != null)
        {
            Color flashColor = isDamage ? damageFlashColor : healFlashColor;

            damageFlashImage.color = flashColor;
            _flashTween = damageFlashImage.DOColor(Color.clear, damageFlashDuration)
                .SetEase(Ease.OutQuad);
        }

        // Анимация изменения здоровья
        if (isDamage)
        {
            // Анимация урона - быстрое уменьшение
            _healthTween = DOTween.To(() => _currentDisplayHealth, x =>
            {
                _currentDisplayHealth = x;
                healthBarSlider.value = x;
            }, newCurrentHealth, damageAnimationDuration)
            .SetEase(damageEase)
            .OnComplete(() => _currentDisplayHealth = newCurrentHealth);
        }
        else if (isHeal)
        {
            // Анимация лечения - плавное увеличение
            _healthTween = DOTween.To(() => _currentDisplayHealth, x =>
            {
                _currentDisplayHealth = x;
                healthBarSlider.value = x;
            }, newCurrentHealth, healAnimationDuration)
            .SetEase(healEase)
            .OnComplete(() => _currentDisplayHealth = newCurrentHealth);
        }
        else
        {
            // Без изменений или установка максимального здоровья
            healthBarSlider.value = newCurrentHealth;
            _currentDisplayHealth = newCurrentHealth;
        }

        // Дополнительная анимация для fill изображения
        if (fillImage != null)
        {
            if (isDamage)
            {
                // Легкое уменьшение размера при уроне
                fillImage.transform.DOScaleX(0.95f, damageAnimationDuration / 2f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => fillImage.transform.DOScaleX(1f, damageAnimationDuration / 2f));
            }
            else if (isHeal)
            {
                // Легкое увеличение размера при лечении
                fillImage.transform.DOScaleX(1.05f, healAnimationDuration / 2f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => fillImage.transform.DOScaleX(1f, healAnimationDuration / 2f));
            }
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении
        if (playerHealth != null && playerHealth.OnHealthChanged != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(RefreshUI);
        }

        // Останавливаем все твины
        _healthTween?.Kill();
        _flashTween?.Kill();
    }

    // Метод для принудительного обновления без анимации
    public void ForceRefreshUI()
    {
        if (playerHealth == null || healthBarSlider == null)
            return;

        _healthTween?.Kill();
        _flashTween?.Kill();

        healthBarSlider.maxValue = playerHealth.GetMaxHealth();
        healthBarSlider.value = playerHealth.GetCurrentHealth();
        _currentDisplayHealth = playerHealth.GetCurrentHealth();

        if (damageFlashImage != null)
        {
            damageFlashImage.color = Color.clear;
        }

        if (fillImage != null)
        {
            fillImage.transform.localScale = Vector3.one;
        }
    }
}