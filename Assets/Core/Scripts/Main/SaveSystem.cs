using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// Сохранение JSON в папку с путём C:\Users\...\AppData\LocalLow\DefaultCompany\Game_Avgust
/// Пока что сохраняется только здоровье и конфигурация оружия и аксессуаров
/// useEncryption шифрует сохранения
/// </summary>
public class SaveSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string saveFileName = "savegame.json";
    [SerializeField] private bool useEncryption = false;
    [SerializeField] private string encryptionKey = "your-encryption-key-here";

    [Header("Стандартные настройки")]
    [SerializeField] private float newHealth;
    [SerializeField] private int newCountKey;
    [SerializeField] private string newWeaponConfigId;
    [SerializeField] private List<string> newAccessoryConfigs = new List<string>();


    private string saveFilePath;
    private GameData currentGameData;



    private void Awake()
    {
        GameManager.Instance.Register(this);
        Initialize();
    }
    private void Initialize()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"Save path: {saveFilePath}");
    }

    public void CreateNewGame()
    {
        currentGameData = new GameData
        {
            currentHealth = newHealth,
            countKey = newCountKey,
            weaponConfigId = this.newWeaponConfigId,
            accessoryConfigIds = this.newAccessoryConfigs
        };
    }

    public void SaveGame()
    {
        if (currentGameData == null)
        {
            Debug.LogWarning("No game data to save!");
            return;
        }

        // Конвертируем в JSON
        string jsonData = JsonUtility.ToJson(currentGameData, true);

        // Шифруем если нужно
        if (useEncryption)
        {
            jsonData = EncryptDecrypt(jsonData);
        }

        // Сохраняем в файл
        try
        {
            File.WriteAllText(saveFilePath, jsonData);
            Debug.Log("Game saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    public bool LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No save file found");
            return false;
        }

        try
        {
            string jsonData = File.ReadAllText(saveFilePath);

            // Дешифруем если нужно
            if (useEncryption)
            {
                jsonData = EncryptDecrypt(jsonData);
            }

            currentGameData = JsonUtility.FromJson<GameData>(jsonData);

            Debug.Log("Game loaded successfully!");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            return false;
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            currentGameData = null;
            Debug.Log("Save file deleted");
        }
    }

    private string EncryptDecrypt(string data)
    {
        char[] dataArray = data.ToCharArray();
        for (int i = 0; i < dataArray.Length; i++)
        {
            dataArray[i] = (char)(dataArray[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return new string(dataArray);
    }

    public GameData GetCurrentGameData() => currentGameData;

    [ContextMenu("Создать новое сохранение")]
    public void ResetSave()
    {
        Initialize();
        DeleteSave();
        CreateNewGame();
    }
}
