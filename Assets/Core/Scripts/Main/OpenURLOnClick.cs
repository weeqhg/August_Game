using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURLOnClick : MonoBehaviour
{
    [SerializeField] private string url = "https://example.com";

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
