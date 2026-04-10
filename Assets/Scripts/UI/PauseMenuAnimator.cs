using System.Collections;
using UnityEngine;

public class PauseMenuAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup dimBackground;
    [SerializeField] private RectTransform panel;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Open Animation")]
    [SerializeField] private float openDuration = 0.22f;
    [SerializeField] private float startScale = 0.82f;
    [SerializeField] private float overshootScale = 1.05f;

    [Header("Close Animation")]
    [SerializeField] private float closeDuration = 0.16f;

    [Header("Dim")]
    [SerializeField] private float dimAlpha = 0.55f;

    [Header("Idle Float")]
    [SerializeField] private bool enableIdleFloat = true;
    [SerializeField] private float floatAmount = 8f;
    [SerializeField] private float floatSpeed = 2.2f;

    private Coroutine animationRoutine;
    private Vector2 panelBasePos;
    private bool isOpen = false;

    private void Awake()
    {
        if (panel != null)
            panelBasePos = panel.anchoredPosition;

        SetHiddenInstant();
    }

    private void Update()
    {
        if (!isOpen || !enableIdleFloat || panel == null)
            return;

        float y = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmount;
        panel.anchoredPosition = panelBasePos + Vector2.up * y;
    }

    public void Show()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        gameObject.SetActive(true);
        animationRoutine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        isOpen = true;

        float elapsed = 0f;

        if (panel != null)
        {
            panel.localScale = Vector3.one * startScale;
            panel.localRotation = Quaternion.Euler(0f, 0f, -4f);
            panel.anchoredPosition = panelBasePos;
        }

        if (dimBackground != null)
            dimBackground.alpha = 0f;

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            float bounce = Mathf.Sin(t * Mathf.PI) * 0.08f;

            if (dimBackground != null)
                dimBackground.alpha = Mathf.Lerp(0f, dimAlpha, easeOut);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, easeOut);

            if (panel != null)
            {
                float scale = Mathf.Lerp(startScale, overshootScale, easeOut);

                if (t > 0.65f)
                {
                    float settleT = (t - 0.65f) / 0.35f;
                    scale = Mathf.Lerp(overshootScale, 1f, settleT);
                }

                panel.localScale = Vector3.one * (scale + bounce * 0.2f);
                panel.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-4f, 0f, easeOut));
            }

            yield return null;
        }

        if (dimBackground != null)
            dimBackground.alpha = dimAlpha;

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;

        if (panel != null)
        {
            panel.localScale = Vector3.one;
            panel.localRotation = Quaternion.identity;
            panel.anchoredPosition = panelBasePos;
        }

        animationRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        isOpen = false;

        float elapsed = 0f;

        float startDim = dimBackground != null ? dimBackground.alpha : 0f;
        float startAlpha = panelCanvasGroup != null ? panelCanvasGroup.alpha : 1f;
        Vector3 startScaleValue = panel != null ? panel.localScale : Vector3.one;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            float easeIn = t * t;

            if (dimBackground != null)
                dimBackground.alpha = Mathf.Lerp(startDim, 0f, easeIn);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, easeIn);

            if (panel != null)
            {
                panel.localScale = Vector3.Lerp(startScaleValue, Vector3.one * 0.9f, easeIn);
                panel.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 3f, easeIn));
            }

            yield return null;
        }

        SetHiddenInstant();
        gameObject.SetActive(false);
        animationRoutine = null;
    }

    private void SetHiddenInstant()
    {
        isOpen = false;

        if (dimBackground != null)
            dimBackground.alpha = 0f;

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        if (panel != null)
        {
            panel.localScale = Vector3.one * startScale;
            panel.localRotation = Quaternion.identity;
            panel.anchoredPosition = panelBasePos;
        }
    }
}