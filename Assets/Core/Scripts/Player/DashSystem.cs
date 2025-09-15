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
    private Queue<ChargeRechargeData> _rechargeQueue = new Queue<ChargeRechargeData>();
    private ChargeRechargeData _currentRecharge;
    private bool _isRecharging;

    // Новое: отслеживаем какие заряды активны/пустые
    private bool[] _chargeStates;

    public int CurrentCharges => _currentDashCharges;
    public int MaxCharges => _maxDashCharges;
    public bool CanDash => _currentDashCharges > 0;
    public bool IsRecharging => _isRecharging;

    private void Start()
    {
        _currentDashCharges = _maxDashCharges;
        _chargeStates = new bool[_maxDashCharges];
        for (int i = 0; i < _maxDashCharges; i++)
        {
            _chargeStates[i] = true; // Все заряды полные
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
        // Ищем самый правый активный заряд (индекс 0 - левый, индекс max-1 - правый)
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

        // Создаем данные для перезарядки
        ChargeRechargeData rechargeData = new ChargeRechargeData
        {
            chargeIndex = usedChargeIndex,
            rechargeTime = _dashRechargeTime,
            currentTime = _dashRechargeTime
        };

        // Добавляем в очередь или начинаем сразу
        if (_isRecharging)
        {
            _rechargeQueue.Enqueue(rechargeData);
        }
        else
        {
            StartRecharge(rechargeData);
        }
    }

    private void StartRecharge(ChargeRechargeData rechargeData)
    {
        _currentRecharge = rechargeData;
        _isRecharging = true;

        // Событие начала перезарядки
        OnRechargeStarted?.Invoke(rechargeData.chargeIndex);
    }

    private void UpdateRecharge()
    {
        if (_isRecharging && _currentRecharge != null)
        {
            _currentRecharge.currentTime -= Time.deltaTime;

            // Отправляем прогресс перезарядки
            OnRechargeProgress?.Invoke(
                _currentRecharge.chargeIndex,
                _dashRechargeTime - _currentRecharge.currentTime,
                _currentRecharge.rechargeTime
            );

            if (_currentRecharge.currentTime <= 0f)
            {
                CompleteRecharge();
            }
        }
    }

    private void CompleteRecharge()
    {
        int rechargedIndex = _currentRecharge.chargeIndex;
        _chargeStates[rechargedIndex] = true;
        _currentDashCharges = Mathf.Min(_currentDashCharges + 1, _maxDashCharges);
        _isRecharging = false;

        // События завершения
        OnRechargeCompleted?.Invoke(rechargedIndex);
        OnChargesChanged?.Invoke(_currentDashCharges, _maxDashCharges);

        // Проверяем, есть ли заряды слева, которые нужно перезарядить в первую очередь
        CheckForLeftCharges();

        // Берем следующую перезарядку из очереди
        if (_rechargeQueue.Count > 0)
        {
            StartRecharge(_rechargeQueue.Dequeue());
        }
    }

    private void CheckForLeftCharges()
    {
        // Ищем самый левый пустой заряд
        int leftmostEmptyIndex = -1;
        for (int i = 0; i < _maxDashCharges; i++)
        {
            if (!_chargeStates[i])
            {
                leftmostEmptyIndex = i;
                break;
            }
        }

        // Если нашли пустой заряд слева и есть текущая перезарядка справа
        if (leftmostEmptyIndex != -1 && _rechargeQueue.Count > 0)
        {
            // Перемещаем перезарядку с правого на левый индекс
            var recharges = _rechargeQueue.ToArray();
            _rechargeQueue.Clear();

            foreach (var recharge in recharges)
            {
                // Если это перезарядка справа от левого пустого, меняем индекс
                if (recharge.chargeIndex > leftmostEmptyIndex)
                {
                    recharge.chargeIndex = leftmostEmptyIndex;
                    leftmostEmptyIndex++; // Сдвигаем индекс для следующей перезарядки
                }
                _rechargeQueue.Enqueue(recharge);
            }
        }
    }

    public void ResetCharges()
    {
        _currentDashCharges = _maxDashCharges;
        _chargeStates = new bool[_maxDashCharges];
        for (int i = 0; i < _maxDashCharges; i++)
        {
            _chargeStates[i] = true;
        }
        _rechargeQueue.Clear();
        _isRecharging = false;
        _currentRecharge = null;
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
}

[System.Serializable]
public class ChargeRechargeData
{
    public int chargeIndex;
    public float rechargeTime;
    public float currentTime;
}