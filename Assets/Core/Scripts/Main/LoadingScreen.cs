using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private Image loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private TextMeshProUGUI currentLevel;
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float progressAnimationDuration = 0.3f;
    [SerializeField] private float textPulseDuration = 1f;

    [Header("Content")]
    [SerializeField] private string[] loadingTips;
    [SerializeField] private float tipChangeInterval = 3f;

    private bool isShowing = false;
    private Coroutine tipsCoroutine;

    private string levelName;
    private void Start()
    {
        InitializeLoadingScreen();
        string lang = PlayerPrefs.GetString("Lang");
        SwitchLanguage(lang);
    }

    private void InitializeLoadingScreen()
    {
        loadingCanvasGroup.alpha = 1f;
        loadingProgressBar.fillAmount = 0f;
    }
    public void SwitchLanguage(string lang)
    {
        switch (lang)
        {
            case "ru":
                levelName = "Уровень: ";
                break;
            case "en":
                levelName = "Level: ";
                break;
            default:
                levelName = "Level: ";
                break;
        }
    }

    public void ShowLoadingScreen()
    {
        if (isShowing) return;

        isShowing = true;
        loadingCanvasGroup.gameObject.SetActive(true);

        // Анимация появления
        loadingCanvasGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartSpinnerAnimation();
                StartTipsAnimation();
            });
    }

    public void HideLoadingScreen()
    {
        if (!isShowing) return;

        // Останавливаем анимации
        if (tipsCoroutine != null)
            StopCoroutine(tipsCoroutine);

        // Анимация исчезновения
        loadingCanvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                loadingCanvasGroup.gameObject.SetActive(false);
                isShowing = false;
                loadingProgressBar.fillAmount = 0f;

                if (loadingSpinner != null)
                    loadingSpinner.SetActive(false);
            });
    }

    public void UpdateProgress(float progress, string description = "")
    {
        progress = Mathf.Clamp01(progress);

        // Анимация прогресс-бара
        loadingProgressBar.DOFillAmount(progress, progressAnimationDuration)
            .SetEase(Ease.OutQuad);

        // Обновление текста прогресса
        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        // Обновление описания если provided
        if (!string.IsNullOrEmpty(description) && loadingText != null)
        {
            loadingText.text = description;
        }
    }

    private void StartSpinnerAnimation()
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true);
            loadingSpinner.transform.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
    }

    private void StartTipsAnimation()
    {
        if (loadingTips != null && loadingTips.Length > 0 && loadingText != null)
        {
            tipsCoroutine = StartCoroutine(TipsAnimationCoroutine());
        }
    }

    private IEnumerator TipsAnimationCoroutine()
    {
        int currentTipIndex = 0;

        while (isShowing)
        {
            // Плавное исчезновение текста
            loadingText.DOFade(0f, 0.3f)
                .SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(0.3f);

            // Смена текста
            loadingText.text = loadingTips[currentTipIndex];
            currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;

            // Плавное появление текста
            loadingText.DOFade(1f, 0.3f)
                .SetEase(Ease.InQuad);

            // Пульсация текста
            loadingText.transform.localScale = Vector3.one;
            loadingText.transform.DOScale(1.05f, textPulseDuration / 2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    loadingText.transform.DOScale(1f, textPulseDuration / 2f)
                        .SetEase(Ease.InQuad);
                });

            yield return new WaitForSeconds(tipChangeInterval);
        }
    }

    public void ShowLevelAnimation(int level)
    {
        Debug.Log(level);
        if (level > 0)
        {
            StartCoroutine(FallbackLevelAnimation(level));
        }
    }

    private IEnumerator FallbackLevelAnimation(int level)
    {
        // Простая анимация текста вверху экрана
        currentLevel.text = levelName + level;

        currentLevel.enabled = true;

        // Анимация появления и исчезновения
        currentLevel.alpha = 0f;
        currentLevel.DOFade(1f, 0.5f);
        currentLevel.transform.DOScale(1.2f, 0.3f).OnComplete(() =>
        {
            currentLevel.transform.DOScale(1f, 0.2f);
        });

        yield return new WaitForSeconds(2f);

        // Исчезновение
        currentLevel.DOFade(0f, 0.5f).OnComplete(() =>
        {
            currentLevel.enabled = false;
        });
    }



    public bool IsVisible()
    {
        return isShowing;
    }

    private void OnDestroy()
    {
        // Очистка всех анимаций
        DOTween.Kill(loadingCanvasGroup);
        DOTween.Kill(loadingProgressBar);
        DOTween.Kill(loadingText);
        DOTween.Kill(backgroundImage);

        if (loadingSpinner != null)
            DOTween.Kill(loadingSpinner.transform);
    }
}