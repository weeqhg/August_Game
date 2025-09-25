using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseAfterActive : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvas;

    private void Start()
    {
        OffCanvas();
    }

    public void OnCanvas()
    {
        canvas.alpha = 1f;
        canvas.blocksRaycasts = true;
        canvas.interactable = true;
    }
    
    public void OffCanvas()
    {
        canvas.alpha = 0f;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }
}
