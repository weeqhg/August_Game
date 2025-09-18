using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LanguageExample : MonoBehaviour
{
    public string ru, en;

    private TextMeshProUGUI textComponent;


    private void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        string lang = PlayerPrefs.GetString("Lang", ru);
        SwitchLanguage(lang);
    }

    public void SwitchLanguage(string lang)
    {
        PlayerPrefs.SetString("Lang", lang);
        switch (lang)
        {
            case "ru":
                textComponent.text = ru;
                break;
            case "en":
                textComponent.text = en;
                break;
            default:
                textComponent.text = en;
                break;
        }
    }

}