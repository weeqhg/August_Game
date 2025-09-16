using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKey : MonoBehaviour
{
    public UnityEvent<int> OnKeyChanged { get; } = new UnityEvent<int>();

    private SaveSystem saveSystem;

    private int countKey;

    public int CurrentCountKey() => countKey;
    private void Start()
    {
        saveSystem = GameManager.Instance.Get<SaveSystem>();
        if (saveSystem != null)
            LoadPlayerData();
    }

    public void AddKey(int value)
    {
        countKey += value;
        OnKeyChanged?.Invoke(countKey);
    }

    public void MinusKey(int value)
    {
        countKey -= value;
        OnKeyChanged?.Invoke(countKey);
    }

    public void LoadPlayerData()
    {
        GameData data = saveSystem.GetCurrentGameData();

        if (data != null)
        {
            countKey = data.countKey;
            OnKeyChanged?.Invoke(countKey);
        }
    }

    public void SaveGameData()
    {
        var gameData = saveSystem.GetCurrentGameData();

        if (gameData != null)
        {
            gameData.countKey = this.countKey;
        }

        saveSystem.SaveGame();
    }
}
