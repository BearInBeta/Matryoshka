using System.Collections;
using TMPro;
using UnityEngine;

public class LevelPassedPanelAnimator : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Main Parts")]
    [SerializeField] private RectTransform titleRoot;
    [SerializeField] private CanvasGroup titleCanvasGroup;

    [SerializeField] private GameObject star1;
    [SerializeField] private GameObject star2;
    [SerializeField] private GameObject star3;

    [SerializeField] private RectTransform timerRoot;
    [SerializeField] private CanvasGroup timerCanvasGroup;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text timerRequirementText;
    [SerializeField] private RectTransform stepsRoot;
    [SerializeField] private CanvasGroup stepsCanvasGroup;
    [SerializeField] private TMP_Text stepsText;
    [SerializeField] private TMP_Text stepsRequirementText;

    [Header("Buttons")]
    [SerializeField] private RectTransform[] buttonRoots;
    [SerializeField] private CanvasGroup[] buttonCanvasGroups;

    [Header("Animation Timings")]
    [SerializeField] private float panelInDuration = 0.28f;
    [SerializeField] private float titleInDuration = 0.35f;
    [SerializeField] private float statInDuration = 0.22f;
    [SerializeField] private float starPopDuration = 0.22f;
    [SerializeField] private float buttonInDuration = 0.18f;
    [SerializeField] private float delayBetweenButtons = 0.08f;
    [SerializeField] private float delayAfterPanel = 0.08f;
    [SerializeField] private float delayAfterTitle = 0.10f;
    [SerializeField] private float delayAfterStar = 0.08f;
    [SerializeField] private float delayAfterDone = 0.22f;
    [Header("Count Durations")]
    [SerializeField] private float timerCountDuration = 0.8f;
    [SerializeField] private float stepsCountDuration = 0.7f;

    [Header("Panel Motion")]
    [SerializeField] private float panelStartScale = 0.82f;
    [SerializeField] private float panelOvershootScale = 1.04f;
    [SerializeField] private float panelStartYOffset = 40f;

    [Header("Title Motion")]
    [SerializeField] private float titleStartYOffset = 50f;
    [SerializeField] private float titleOvershootScale = 1.08f;

    [Header("Stat Motion")]
    [SerializeField] private float statStartXOffset = -40f;

    [Header("Button Motion")]
    [SerializeField] private float buttonStartYOffset = 25f;

    [Header("SFX")]
    [SerializeField] private string panelOpenSfx = "ui_panel";
    [SerializeField] private string titleSfx = "ui_title";
    [SerializeField] private string starSfx = "star";
    [SerializeField] private string counterStartSfx = "ui_counter";
    [SerializeField] private string counterTickSfx = "tick";
    [SerializeField] private string buttonSfx = "ui_button";
    [SerializeField] private string perfectSfx = "success";

    private Coroutine playRoutine;

    private Vector2 panelBasePos;
    private Vector2 titleBasePos;
    private Vector2 timerBasePos;
    private Vector2 stepsBasePos;
    private Vector2[] buttonBasePositions;

    private void Awake()
    {
        if (panelRoot != null) panelBasePos = panelRoot.anchoredPosition;
        if (titleRoot != null) titleBasePos = titleRoot.anchoredPosition;
        if (timerRoot != null) timerBasePos = timerRoot.anchoredPosition;
        if (stepsRoot != null) stepsBasePos = stepsRoot.anchoredPosition;

        if (buttonRoots != null)
        {
            buttonBasePositions = new Vector2[buttonRoots.Length];
            for (int i = 0; i < buttonRoots.Length; i++)
            {
                if (buttonRoots[i] != null)
                    buttonBasePositions[i] = buttonRoots[i].anchoredPosition;
            }
        }

        HideAllImmediate();
    }

    public void Play(LevelData currentLevel, float elapsedTime, int currentSteps)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        gameObject.SetActive(true);
        playRoutine = StartCoroutine(PlayRoutine(currentLevel, elapsedTime, currentSteps));
    }

    private IEnumerator PlayRoutine(LevelData currentLevel, float elapsedTime, int currentSteps)
    {
        HideAllImmediate();

        yield return AnimatePanelIn();

        yield return Wait(delayAfterPanel);

        yield return AnimateTitleIn();

        yield return Wait(delayAfterTitle);

        yield return AnimateStar(star1);

        yield return Wait(delayAfterStar);
        timerRequirementText.text = "≤ " + currentLevel.minTime;
        stepsRequirementText.text = "≤ " + currentLevel.minSteps;

        yield return AnimateStatIn(timerRoot, timerCanvasGroup, timerBasePos);
        yield return CountTimer(elapsedTime);

        int flooredTime = Mathf.FloorToInt(elapsedTime);
        if (flooredTime <= currentLevel.minTime)
        {
            yield return AnimateStar(star2);
            
        }

        yield return Wait(delayAfterStar);

        yield return AnimateStatIn(stepsRoot, stepsCanvasGroup, stepsBasePos);
        yield return CountSteps(currentSteps);

        if (currentSteps <= currentLevel.minSteps)
        {
            yield return AnimateStar(star3);
            
        }

        if (currentSteps <= currentLevel.minSteps && flooredTime <= currentLevel.minTime)
        {
            PlaySfx(perfectSfx);
        }

        yield return Wait(delayAfterDone);



        for (int i = 0; i < buttonRoots.Length; i++)
        {
            if (buttonRoots[i] == null || buttonCanvasGroups[i] == null)
                continue;

            yield return AnimateButtonIn(buttonRoots[i], buttonCanvasGroups[i], buttonBasePositions[i]);
            yield return Wait(delayBetweenButtons);
        }

        playRoutine = null;
    }

    private void HideAllImmediate()
    {
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        if (panelRoot != null)
        {
            panelRoot.localScale = Vector3.one * panelStartScale;
            panelRoot.anchoredPosition = panelBasePos + Vector2.down * panelStartYOffset;
        }

        SetHidden(titleRoot, titleCanvasGroup, titleBasePos + Vector2.up * titleStartYOffset);
        SetHidden(timerRoot, timerCanvasGroup, timerBasePos + Vector2.left * statStartXOffset);
        SetHidden(stepsRoot, stepsCanvasGroup, stepsBasePos + Vector2.left * statStartXOffset);

        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);

        if (timerText != null) timerText.text = "0";
        if (stepsText != null) stepsText.text = "0";

        if (buttonRoots != null && buttonCanvasGroups != null)
        {
            for (int i = 0; i < Mathf.Min(buttonRoots.Length, buttonCanvasGroups.Length); i++)
            {
                if (buttonRoots[i] == null || buttonCanvasGroups[i] == null)
                    continue;

                buttonCanvasGroups[i].alpha = 0f;
                buttonRoots[i].anchoredPosition = buttonBasePositions[i] + Vector2.down * buttonStartYOffset;
            }
        }
    }

    private void SetHidden(RectTransform rect, CanvasGroup cg, Vector2 hiddenPos)
    {
        if (cg != null)
            cg.alpha = 0f;

        if (rect != null)
        {
            rect.anchoredPosition = hiddenPos;
            rect.localScale = Vector3.one * 0.9f;
        }
    }

    private IEnumerator AnimatePanelIn()
    {
        PlaySfx(panelOpenSfx);

        float t = 0f;
        while (t < panelInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / panelInDuration);
            float ease = EaseOutBack01(p);

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = p;

            if (panelRoot != null)
            {
                panelRoot.anchoredPosition = Vector2.LerpUnclamped(
                    panelBasePos + Vector2.down * panelStartYOffset,
                    panelBasePos,
                    ease
                );

                float scale = Mathf.LerpUnclamped(panelStartScale, panelOvershootScale, ease);

                if (p > 0.75f)
                {
                    float settle = Mathf.InverseLerp(0.75f, 1f, p);
                    scale = Mathf.Lerp(panelOvershootScale, 1f, settle);
                }

                panelRoot.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1f;
        if (panelRoot != null)
        {
            panelRoot.anchoredPosition = panelBasePos;
            panelRoot.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateTitleIn()
    {
        PlaySfx(titleSfx);

        float t = 0f;
        while (t < titleInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / titleInDuration);
            float ease = EaseOutBack01(p);

            if (titleCanvasGroup != null)
                titleCanvasGroup.alpha = p;

            if (titleRoot != null)
            {
                titleRoot.anchoredPosition = Vector2.LerpUnclamped(
                    titleBasePos + Vector2.up * titleStartYOffset,
                    titleBasePos,
                    ease
                );

                float scale = Mathf.LerpUnclamped(0.75f, titleOvershootScale, ease);

                if (p > 0.7f)
                {
                    float settle = Mathf.InverseLerp(0.7f, 1f, p);
                    scale = Mathf.Lerp(titleOvershootScale, 1f, settle);
                }

                titleRoot.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (titleCanvasGroup != null) titleCanvasGroup.alpha = 1f;
        if (titleRoot != null)
        {
            titleRoot.anchoredPosition = titleBasePos;
            titleRoot.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateStatIn(RectTransform root, CanvasGroup cg, Vector2 basePos)
    {
        PlaySfx(counterStartSfx);

        float t = 0f;
        while (t < statInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / statInDuration);
            float ease = EaseOutBack01(p);

            if (cg != null)
                cg.alpha = p;

            if (root != null)
            {
                root.anchoredPosition = Vector2.LerpUnclamped(
                    basePos + Vector2.left * statStartXOffset,
                    basePos,
                    ease
                );

                float scale = Mathf.LerpUnclamped(0.9f, 1.05f, ease);
                if (p > 0.75f)
                {
                    float settle = Mathf.InverseLerp(0.75f, 1f, p);
                    scale = Mathf.Lerp(1.05f, 1f, settle);
                }

                root.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (cg != null) cg.alpha = 1f;
        if (root != null)
        {
            root.anchoredPosition = basePos;
            root.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateButtonIn(RectTransform root, CanvasGroup cg, Vector2 basePos)
    {
        PlaySfx(buttonSfx);

        float t = 0f;
        while (t < buttonInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / buttonInDuration);
            float ease = EaseOutBack01(p);

            cg.alpha = p;
            root.anchoredPosition = Vector2.LerpUnclamped(
                basePos + Vector2.down * buttonStartYOffset,
                basePos,
                ease
            );

            float scale = Mathf.LerpUnclamped(0.85f, 1.06f, ease);
            if (p > 0.72f)
            {
                float settle = Mathf.InverseLerp(0.72f, 1f, p);
                scale = Mathf.Lerp(1.06f, 1f, settle);
            }

            root.localScale = Vector3.one * scale;

            yield return null;
        }

        cg.alpha = 1f;
        root.anchoredPosition = basePos;
        root.localScale = Vector3.one;
    }

    private IEnumerator AnimateStar(GameObject starObj)
    {
        if (starObj == null)
            yield break;

        PlaySfx(starSfx);

        starObj.SetActive(true);

        Transform tr = starObj.transform;
        tr.localScale = Vector3.one * 0.2f;
        tr.localRotation = Quaternion.Euler(0f, 0f, -20f);

        float t = 0f;
        while (t < starPopDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / starPopDuration);
            float ease = EaseOutBack01(p);

            float scale = Mathf.LerpUnclamped(0.2f, 1.18f, ease);
            if (p > 0.7f)
            {
                float settle = Mathf.InverseLerp(0.7f, 1f, p);
                scale = Mathf.Lerp(1.18f, 1f, settle);
            }

            tr.localScale = Vector3.one * scale;
            tr.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-20f, 0f, ease));

            yield return null;
        }

        tr.localScale = Vector3.one;
        tr.localRotation = Quaternion.identity;
    }

    private IEnumerator CountTimer(float elapsedTime)
    {
        int finalValue = Mathf.FloorToInt(elapsedTime);
        yield return CountNumber(timerText, finalValue, timerCountDuration);
    }

    private IEnumerator CountSteps(int stepCount)
    {
        yield return CountNumber(stepsText, stepCount, stepsCountDuration);
    }

    private IEnumerator CountNumber(TMP_Text label, int finalValue, float duration)
    {
        if (label == null)
            yield break;

        int lastShown = -1;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            int value = Mathf.RoundToInt(Mathf.Lerp(0, finalValue, EaseOutCubic(p)));

            if (value != lastShown)
            {
                label.text = value.ToString();
                lastShown = value;

                PlaySfx(counterTickSfx);
            }

            yield return null;
        }

        label.text = finalValue.ToString();
    }

    private string FormatSeconds(int totalSeconds)
    {
        int seconds = totalSeconds;
        return $"{seconds:00}";
    }

    private IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void PlaySfx(string soundName)
    {
        SFXManager sfx = FindFirstObjectByType<SFXManager>();
        if (sfx != null && !string.IsNullOrEmpty(soundName))
            sfx.PlayClip(soundName);
    }

    private float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    private float EaseOutBack01(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}