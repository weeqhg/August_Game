using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HelpUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI helpDash;
    [SerializeField] private TextMeshProUGUI helpAccessory;

    private int isHelpDash;
    private int isHelpAccessory;
    private void Start()
    {
        isHelpDash = PlayerPrefs.GetInt("HelpDash", 0);
        isHelpAccessory = PlayerPrefs.GetInt("HelpAccessory", 0);

        if (isHelpAccessory == 0)
            helpAccessory.enabled = true;
        else helpAccessory.enabled = false;

        if (isHelpDash == 0)
            helpDash.enabled = true;
        else helpDash.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            PlayerPrefs.SetInt("HelpDash", 1);
            helpDash.enabled = false;
            isHelpDash = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            PlayerPrefs.SetInt("HelpAccessory", 1);
            helpAccessory.enabled = false;
            isHelpAccessory = 1;
        }

        if (isHelpDash == 1 && isHelpAccessory == 1)
        {
            enabled = false;
        }
    }
}
