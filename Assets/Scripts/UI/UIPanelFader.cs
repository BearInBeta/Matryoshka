using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private bool ignoreTimeScale = true;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        FadeIn();
    }

    public void FadeIn()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        canvasGroup.alpha = 0f;
        fadeRoutine = StartCoroutine(FadeRoutine(1f, false));
    }

    public void FadeOut()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        if(gameObject.activeSelf)
            fadeRoutine = StartCoroutine(FadeRoutine(0f, true));
    }

    private IEnumerator FadeRoutine(float target, bool deactivateAfter)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (time < fadeDuration)
        {
            time += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = time / fadeDuration;

            canvasGroup.alpha = Mathf.Lerp(start, target, t);

            yield return null;
        }

        canvasGroup.alpha = target;

        if (target == 1f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (deactivateAfter)
            gameObject.SetActive(false);

        fadeRoutine = null;
    }
}