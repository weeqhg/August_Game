using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class DashSystem : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private int _maxDashCharges = 3;
    [SerializeField] private float _dashRechargeTime = 2f;

    [Header("Events")]
    public UnityEvent<int, int> OnChargesChanged; // currentCharges, maxCharges
    public UnityEvent<int, float, float> OnRechargeProgress; // chargeIndex, currentTime, totalTime
    public UnityEvent<int> OnRechargeStarted; // chargeIndex
    public UnityEvent<int> OnRechargeCompleted; // chargeIndex
    public UnityEvent<int> OnChargeUsed; // chargeIndex

    private int _currentDashCharges;
    private float _currentRechargeProgress = 0f;
    private int _currentRechargingIndex = -1;
    private bool _isRecharging = false;

    // Отслеживаем состояние каждого заряда
    private bool[] _chargeStates;

    public int CurrentCharges => _currentDashCharges;
    public int MaxCharges => _maxDashCharges;
    public bool CanDash => _currentDashCharges > 0;
    public bool IsRecharging => _isRecharging;

    private void Start()
    {
        InitializeCharges();
    }

    private void InitializeCharges()
    {
        _currentDashCharges = _maxDashCharges;
        _chargeStates = new bool[_maxDashCharges];

        for (int i = 0; i < _maxDashCharges; i++)
        {
            _chargeStates[i] = true;
        }

        OnChargesChanged?.Invoke(_currentDashCharges, _maxDashCharges);
    }

    private void Update()
    {
        UpdateRecharge();
    }

    public bool TryUseDash()
    {
        if (_currentDashCharges > 0)
        {
            UseDash();
            return true;
        }
        return false;
    }

    private void UseDash()
    {
        // Используем самый правый активный заряд
        int usedChargeIndex = -1;
        for (int i = _maxDashCharges - 1; i >= 0; i--)
        {
            if (_chargeStates[i])
            {
                usedChargeIndex = i;
                _chargeStates[i] = false;
                break;
            }
        }

        if (usedChargeIndex == -1) return;

        _currentDashCharges--;

        // Событие использования заряда
        OnChargeUsed?.Invoke(usedChargeIndex);
        OnChargesChanged?.Invoke(_currentDashCharges, _maxDashCharges);

        // Если уже идет перезарядка, прерываем её и сохраняем прогресс
        if (_isRecharging)
        {
            InterruptRecharge();
        }

        // Начинаем перезарядку на самом левом пустом слоте
        StartRecharge();
    }

    private void StartRecharge()
    {
        // Находим самый левый пустой слот
        int targetIndex = -1;
        for (int i = 0; i < _maxDashCharges; i++)
        {
            if (!_chargeStates[i])
            {
                targetIndex = i;
                break;
            }
        }

        // Если нет пустых слотов, выходим
        if (targetIndex == -1)
        {
            _isRecharging = false;
            return;
        }

        _currentRechargingIndex = targetIndex;
        _isRecharging = true;

        // Событие начала перезарядки
        OnRechargeStarted?.Invoke(_currentRechargingIndex);
    }

    private void InterruptRecharge()
    {
        // Сохраняем текущий прогресс перезарядки
        // Прогресс уже хранится в _currentRechargeProgress
        _isRecharging = false;
        _currentRechargingIndex = -1;
    }

    private void UpdateRecharge()
    {
        if (_isRecharging)
        {
            _currentRechargeProgress += Time.deltaTime;
            float progress = _currentRechargeProgress / _dashRechargeTime;

            // Отправляем прогресс перезарядки
            OnRechargeProgress?.Invoke(
                _currentRechargingIndex,
                _currentRechargeProgress,
                _dashRechargeTime
            );

            if (_currentRechargeProgress >= _dashRechargeTime)
            {
                CompleteCurrentRecharge();
            }
        }
    }

    private void CompleteCurrentRecharge()
    {
        _chargeStates[_currentRechargingIndex] = true;
        _currentDashCharges = Mathf.Min(_currentDashCharges + 1, _maxDashCharges);
        _currentRechargeProgress = 0f;

        // События завершения
        OnRechargeCompleted?.Invoke(_currentRechargingIndex);
        OnChargesChanged?.Invoke(_currentDashCharges, _maxDashCharges);

        // Сбрасываем состояние перезарядки
        _isRecharging = false;
        _currentRechargingIndex = -1;

        // Проверяем, есть ли еще пустые слоты для перезарядки
        StartRecharge();
    }

    public void ResetCharges()
    {
        _currentDashCharges = _maxDashCharges;
        _isRecharging = false;
        _currentRechargingIndex = -1;
        _currentRechargeProgress = 0f;

        for (int i = 0; i < _maxDashCharges; i++)
        {
            _chargeStates[i] = true;
        }

        OnChargesChanged?.Invoke(_currentDashCharges, _maxDashCharges);
    }

    public void SetMaxCharges(int maxCharges)
    {
        _maxDashCharges = maxCharges;
        _currentDashCharges = Mathf.Min(_currentDashCharges, _maxDashCharges);

        // Обновляем массив состояний
        bool[] newChargeStates = new bool[maxCharges];

        for (int i = 0; i < Mathf.Min(_chargeStates.Length, maxCharges); i++)
        {
            newChargeStates[i] = _chargeStates[i];
        }

        for (int i = _chargeStates.Length; i < maxCharges; i++)
        {
            newChargeStates[i] = true;
        }

        _chargeStates = newChargeStates;

        OnChargesChanged?.Invoke(_currentDashCharges, _maxDashCharges);
    }

    // Для отладки
    public string GetDebugInfo()
    {
        string info = $"Charges: {_currentDashCharges}/{_maxDashCharges}\n";
        info += $"Recharging: {_isRecharging}, Current Index: {_currentRechargingIndex}\n";
        info += $"Progress: {_currentRechargeProgress:F2}/{_dashRechargeTime:F2}\n";

        for (int i = 0; i < _maxDashCharges; i++)
        {
            info += $"Charge {i}: {_chargeStates[i]}\n";
        }

        return info;
    }
}