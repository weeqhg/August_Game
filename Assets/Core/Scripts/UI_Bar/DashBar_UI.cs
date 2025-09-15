using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class DashBar_UI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DashSystem _dashSystem;
    [SerializeField] private GameObject _chargeSliderPrefab;
    [SerializeField] private GameObject _dashObject;

    [Header("UI Settings")]
    [SerializeField] private float _spacing = 10f;
    [SerializeField] private Color _activeColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color _rechargingColor = new Color(0.4f, 0.4f, 0.4f);
    [SerializeField] private Color _emptyColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("Animation Settings")]
    [SerializeField] private float _pulseIntensity = 1.2f;
    [SerializeField] private float _pulseDuration = 0.2f;
    [SerializeField] private float _shakeIntensity = 5f;
    [SerializeField] private float _shakeDuration = 0.3f;

    private List<Slider> _chargeSliders = new List<Slider>();
    private List<Image> _fillImages = new List<Image>();
    private RectTransform _canvasRect;

    private int _currentRechargingIndex = -1;
    private float _currentRechargeProgress = 0f;
    private float _currentRechargeTotalTime = 0f;

    public void Initialize(DashSystem newDashSystem)
    {
        _dashSystem = newDashSystem;

        if (_dashSystem != null)
        {
            UnsubscribeFromEvents();
            InitializeSliders();
            SubscribeToEvents();
            UpdateAllSliders();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSliders()
    {
        foreach (Transform child in _dashObject.transform)
        {
            Destroy(child.gameObject);
        }
        _chargeSliders.Clear();
        _fillImages.Clear();
        _currentRechargingIndex = -1;
        _currentRechargeProgress = 0f;
        _currentRechargeTotalTime = 0f;

        if (_chargeSliderPrefab == null) return;

        for (int i = 0; i < _dashSystem.MaxCharges; i++)
        {
            GameObject sliderObj = Instantiate(_chargeSliderPrefab, _dashObject.transform);
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.interactable = false;
            Image fillImage = slider.fillRect.GetComponent<Image>();

            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            if (_canvasRect == null)
            {
                _canvasRect = _dashObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            }

            rt.anchoredPosition = new Vector2(i * (rt.sizeDelta.x + _spacing), 0);

            _chargeSliders.Add(slider);
            _fillImages.Add(fillImage);
            fillImage.color = _activeColor;
        }
    }

    private void SubscribeToEvents()
    {
        _dashSystem.OnChargesChanged.AddListener(OnChargesChanged);
        _dashSystem.OnRechargeProgress.AddListener(OnRechargeProgress);
        _dashSystem.OnRechargeStarted.AddListener(OnRechargeStarted);
        _dashSystem.OnRechargeCompleted.AddListener(OnRechargeCompleted);
        _dashSystem.OnChargeUsed.AddListener(OnChargeUsed);
    }

    private void UnsubscribeFromEvents()
    {
        if (_dashSystem != null)
        {
            _dashSystem.OnChargesChanged.RemoveListener(OnChargesChanged);
            _dashSystem.OnRechargeProgress.RemoveListener(OnRechargeProgress);
            _dashSystem.OnRechargeStarted.RemoveListener(OnRechargeStarted);
            _dashSystem.OnRechargeCompleted.RemoveListener(OnRechargeCompleted);
            _dashSystem.OnChargeUsed.RemoveListener(OnChargeUsed);
        }
    }

    private void OnChargesChanged(int currentCharges, int maxCharges)
    {
        UpdateAllSliders();
    }

    private void OnRechargeProgress(int chargeIndex, float currentTime, float totalTime)
    {
        _currentRechargeProgress = currentTime;
        _currentRechargeTotalTime = totalTime;

        if (_currentRechargingIndex >= 0 && _currentRechargingIndex < _chargeSliders.Count)
        {
            float progress = currentTime / totalTime;
            _chargeSliders[_currentRechargingIndex].value = progress;
        }
    }

    private void OnRechargeStarted(int chargeIndex)
    {
        // ИСПРАВЛЕНО: Используем переданный индекс вместо поиска
        StartRecharge(chargeIndex);
    }

    private void StartRecharge(int sliderIndex)
    {
        if (_currentRechargingIndex != -1)
        {
            InterruptCurrentRecharge();
        }

        _currentRechargingIndex = sliderIndex;
        _currentRechargeProgress = 0f;

        if (_currentRechargingIndex >= 0 && _currentRechargingIndex < _fillImages.Count)
        {
            _fillImages[_currentRechargingIndex].color = _rechargingColor;
            _chargeSliders[_currentRechargingIndex].value = 0f;
        }
    }

    private void OnRechargeCompleted(int chargeIndex)
    {
        if (_currentRechargingIndex >= 0 && _currentRechargingIndex < _fillImages.Count)
        {
            _fillImages[_currentRechargingIndex].color = _activeColor;
            _chargeSliders[_currentRechargingIndex].value = 1f;
            PulseSlider(_currentRechargingIndex);
        }

        _currentRechargingIndex = -1;

        // ИСПРАВЛЕНО: Больше не ищем следующий слайдер автоматически
        // DashSystem сам вызовет OnRechargeStarted для следующего заряда
    }

    private void OnChargeUsed(int chargeIndex)
    {
        if (chargeIndex >= 0 && chargeIndex < _chargeSliders.Count)
        {
            _chargeSliders[chargeIndex].value = 0f;
            _fillImages[chargeIndex].color = _emptyColor;
            ShakeSlider(chargeIndex);

            // Если использованный заряд был текущим перезаряжаемым, прерываем перезарядку
            if (chargeIndex == _currentRechargingIndex)
            {
                InterruptCurrentRecharge();
            }

            // Если использованный заряд левее текущего перезаряжаемого,
            // переключаем перезарядку на использованный заряд
            else if (chargeIndex < _currentRechargingIndex && _currentRechargingIndex != -1)
            {
                // Сохраняем текущий прогресс перезарядки
                float currentProgress = _chargeSliders[_currentRechargingIndex].value;

                // Прерываем текущую перезарядку
                InterruptCurrentRecharge();

                // Начинаем перезарядку использованного заряда
                StartRecharge(chargeIndex);

                // Устанавливаем сохраненный прогресс
                _chargeSliders[chargeIndex].value = currentProgress;
            }

            // Если нет активной перезарядки, начинаем перезарядку использованного заряда
            else if (_currentRechargingIndex == -1)
            {
                StartRecharge(chargeIndex);
            }
        }
    }

    private void InterruptCurrentRecharge()
    {
        if (_currentRechargingIndex >= 0 && _currentRechargingIndex < _fillImages.Count)
        {
            _fillImages[_currentRechargingIndex].color = _emptyColor;
            _chargeSliders[_currentRechargingIndex].value = 0f;
        }

        _currentRechargingIndex = -1;
    }

    private void UpdateAllSliders()
    {
        for (int i = 0; i < _chargeSliders.Count; i++)
        {
            if (i == _currentRechargingIndex) continue;

            bool shouldBeFull = i < _dashSystem.CurrentCharges;

            if (shouldBeFull)
            {
                _chargeSliders[i].value = 1f;
                _fillImages[i].color = _activeColor;
            }
            else
            {
                _chargeSliders[i].value = 0f;
                _fillImages[i].color = _emptyColor;
            }
        }
    }

    private void PulseSlider(int sliderIndex)
    {
        if (sliderIndex >= 0 && sliderIndex < _chargeSliders.Count)
        {
            Transform sliderTransform = _chargeSliders[sliderIndex].transform;
            sliderTransform.DOScale(Vector3.one * _pulseIntensity, _pulseDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => sliderTransform.DOScale(Vector3.one, _pulseDuration));
        }
    }

    private void ShakeSlider(int sliderIndex)
    {
        if (sliderIndex >= 0 && sliderIndex < _chargeSliders.Count)
        {
            Transform sliderTransform = _chargeSliders[sliderIndex].transform;
            sliderTransform.DOShakePosition(_shakeDuration, _shakeIntensity);
        }
    }

    public void RefreshUI()
    {
        InitializeSliders();
        UpdateAllSliders();
    }
}