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
    private float _savedProgress = 0f;

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
        _savedProgress = 0f;

        if (_chargeSliderPrefab == null) return;

        for (int i = 0; i < _dashSystem.MaxCharges; i++)
        {
            GameObject sliderObj = Instantiate(_chargeSliderPrefab, _dashObject.transform);
            Slider slider = sliderObj.GetComponent<Slider>();
            Image fillImage = slider.fillRect.GetComponent<Image>();

            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            // Позиционирование
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
        if (chargeIndex >= 0 && chargeIndex < _chargeSliders.Count)
        {
            float progress = currentTime / totalTime;
            _chargeSliders[chargeIndex].value = progress;
            _fillImages[chargeIndex].color = _rechargingColor;

            // Сохраняем прогресс для возможного переноса
            _savedProgress = progress;
        }
    }

    private void OnRechargeStarted(int chargeIndex)
    {
        if (chargeIndex >= 0 && chargeIndex < _chargeSliders.Count)
        {
            _currentRechargingIndex = chargeIndex;

            // Если есть сохраненный прогресс, используем его
            if (_savedProgress > 0f)
            {
                _chargeSliders[chargeIndex].value = _savedProgress;
            }
            else
            {
                _chargeSliders[chargeIndex].value = 0f;
            }

            _fillImages[chargeIndex].color = _rechargingColor;
        }
    }

    private void OnRechargeCompleted(int chargeIndex)
    {
        if (chargeIndex >= 0 && chargeIndex < _chargeSliders.Count)
        {
            _fillImages[chargeIndex].color = _activeColor;
            _chargeSliders[chargeIndex].value = 1f;
            PulseSlider(chargeIndex);
        }

        _currentRechargingIndex = -1;
        _savedProgress = 0f;
    }

    private void OnChargeUsed(int chargeIndex)
    {
        // При использовании даша сохраняем текущий прогресс перезарядки
        if (_currentRechargingIndex >= 0)
        {
            // Сохраняем прогресс текущей перезарядки
            _savedProgress = _chargeSliders[_currentRechargingIndex].value;

            // Сбрасываем текущий перезаряжаемый слайдер
            _chargeSliders[_currentRechargingIndex].value = 0f;
            _fillImages[_currentRechargingIndex].color = _emptyColor;
        }

        // Обновляем все слайдеры
        UpdateAllSliders();

        if (chargeIndex >= 0 && chargeIndex < _chargeSliders.Count)
        {
            ShakeSlider(chargeIndex);
        }
    }

    private void UpdateAllSliders()
    {
        for (int i = 0; i < _chargeSliders.Count; i++)
        {
            // Если это текущий перезаряжаемый слайдер, пропускаем его
            if (i == _currentRechargingIndex) continue;

            // Определяем состояние слайдера
            bool isActive = i < _dashSystem.CurrentCharges;

            if (isActive)
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

    // Для отладки
    public string GetDebugInfo()
    {
        string info = $"Recharging Index: {_currentRechargingIndex}\n";
        info += $"Saved Progress: {_savedProgress:F2}\n";
        info += $"Current Charges: {_dashSystem.CurrentCharges}/{_dashSystem.MaxCharges}\n";
        return info;
    }
}