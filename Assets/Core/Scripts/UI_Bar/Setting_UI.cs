using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setting_UI : MonoBehaviour
{
    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isActive = false;
    private bool isWait = true;


    private void Start()
    {
        settingCanvas.enabled = false;

        Invoke("WaitForLoad", 0.7f);
    }

    public void ToggleMenu()
    {
        isActive = !isActive;

        if (isActive)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    private void OpenMenu()
    {
        settingCanvas.enabled = true;

        Time.timeScale = 0f; // Пауза игры
    }

    private void CloseMenu()
    {
        settingCanvas.enabled = false;

        Time.timeScale = 1f; // Возобновление игры
    }

    // Для кнопки UI
    public void OnSettingsButtonClicked()
    {
        ToggleMenu();
    }

    private void Update()
    {
        // Открытие/закрытие по клавише ESC
        if (Input.GetKeyDown(KeyCode.Escape) && !isWait)
        {
            ToggleMenu();
        }
    }

    private void WaitForLoad()
    {
        isWait = false;
    }
}
