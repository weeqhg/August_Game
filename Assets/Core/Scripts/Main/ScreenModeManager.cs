using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenModeManager : MonoBehaviour
{
    [System.Serializable]
    public enum ScreenMode
    {
        Fullscreen,
        Borderless,
        Windowed
    }

    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Button applyButton;

    [Header("Settings")]
    [SerializeField] private ScreenMode currentScreenMode = ScreenMode.Fullscreen;
    [SerializeField] private Vector2Int windowedResolution = new Vector2Int(1280, 720);

    private Resolution[] availableResolutions;
    private bool settingsChanged = false;

    private void Start()
    {
        InitializeResolutions();
        LoadSettings();
        SetupUI();
    }

    private void InitializeResolutions()
    {
        availableResolutions = Screen.resolutions;
        System.Array.Reverse(availableResolutions); // Сортируем от высоких к низким
    }

    private void SetupUI()
    {
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(new System.Collections.Generic.List<string> 
            {
                "Full Screen",
                "Borderless",
                "Windowed"
            });
            screenModeDropdown.value = (int)currentScreenMode;
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var resolutionOptions = new System.Collections.Generic.List<string>();
            
            foreach (var res in availableResolutions)
            {
                resolutionOptions.Add($"{res.width} x {res.height} ({res.refreshRate}Hz)");
            }
            
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySettings);
            applyButton.interactable = false;
        }
    }

    private void OnScreenModeChanged(int index)
    {
        currentScreenMode = (ScreenMode)index;
        settingsChanged = true;
        UpdateApplyButton();
    }

    private void OnResolutionChanged(int index)
    {
        settingsChanged = true;
        UpdateApplyButton();
    }

    private void UpdateApplyButton()
    {
        if (applyButton != null)
        {
            applyButton.interactable = settingsChanged;
        }
    }

    public void ApplySettings()
    {
        ApplyScreenMode(currentScreenMode);
        SaveSettings();
        settingsChanged = false;
        UpdateApplyButton();
    }

    public void ApplyScreenMode(ScreenMode mode)
    {
        switch (mode)
        {
            case ScreenMode.Fullscreen:
                Screen.SetResolution(Screen.currentResolution.width, 
                                   Screen.currentResolution.height, 
                                   FullScreenMode.FullScreenWindow);
                break;

            case ScreenMode.Borderless:
                Screen.SetResolution(Screen.currentResolution.width,
                                   Screen.currentResolution.height,
                                   FullScreenMode.MaximizedWindow);
                break;

            case ScreenMode.Windowed:
                if (resolutionDropdown != null)
                {
                    var selectedResolution = availableResolutions[resolutionDropdown.value];
                    Screen.SetResolution(selectedResolution.width, 
                                       selectedResolution.height, 
                                       FullScreenMode.Windowed);
                }
                else
                {
                    Screen.SetResolution(windowedResolution.x, 
                                       windowedResolution.y, 
                                       FullScreenMode.Windowed);
                }
                break;
        }

        Debug.Log($"Режим экрана изменен на: {mode}");
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ScreenMode", (int)currentScreenMode);
        
        if (resolutionDropdown != null)
        {
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        }
        
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ScreenMode"))
        {
            currentScreenMode = (ScreenMode)PlayerPrefs.GetInt("ScreenMode");
        }

        if (resolutionDropdown != null && PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex");
            if (savedIndex < availableResolutions.Length)
            {
                resolutionDropdown.value = savedIndex;
            }
        }
    }
}