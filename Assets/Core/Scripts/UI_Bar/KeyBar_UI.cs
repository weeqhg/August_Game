using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyBar_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textCountKey;


    public void Initialize(PlayerKey newPlayerKey)
    {

        if (newPlayerKey.OnKeyChanged != null)
        {
            newPlayerKey.OnKeyChanged.RemoveListener(RefreshUI);
        }

        newPlayerKey.OnKeyChanged.AddListener(RefreshUI);
    }


    public void RefreshUI(int value)
    {
        textCountKey.text = value.ToString();
    }
}
