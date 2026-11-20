using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays in the main menu scene after the secret level: fades in from black over the menu
/// background, shows "fin.", then "thank you for playing", scrolls the credits, and finally
/// hands control back to the main menu. Built entirely at runtime.
/// </summary>
public class EndingSequence : MonoBehaviour
{
    [Header("Timing")]
    public float fadeFromBlack = 2.0f;
    public float textFadeIn = 1.6f;
    public float finHold = 2.2f;
    public float crossFade = 1.6f;
    public float thanksHold = 2.6f;
    public float textFadeOut = 1.2f;
    public float creditsSpeed = 110f;      // reference pixels per second (1920x1080 canvas)
    public float finalFade = 1.2f;

    [Header("Look")]
    public Color textColour = Color.white;
    public Color tint = new Color(0.08f, 0.04f, 0.14f, 0.35f);

    private RectTransform canvasRect;

    public void Play(IList<TMP_Text> creditsSources, Action onFinished)
    {
        StartCoroutine(Run(creditsSources, onFinished));
    }

    private IEnumerator Run(IList<TMP_Text> creditsSources, Action onFinished)
    {
        TMP_Text creditsSource = creditsSources != null && creditsSources.Count > 0 ? creditsSources[0] : null;
        // ---- build the overlay ----
        GameObject canvasGo = new GameObject("EndingSequence", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = (RectTransform)canvasGo.transform;

        Image tintImage = CreateImage("Tint", canvasGo.transform, tint);           // keeps text readable
        Image black = CreateImage("Black", canvasGo.transform, Color.black);       // continues the game's fade

        TMP_FontAsset font = creditsSource != null ? creditsSource.font : null;
        TMP_Text fin = CreateText("Fin", canvasGo.transform, font, "fin.", 140f, FontStyles.Italic);
        TMP_Text thanks = CreateText("Thanks", canvasGo.transform, font, "thank you for playing", 84f, FontStyles.Normal);
        TMP_Text credits = CreateText("Credits", canvasGo.transform, font, BuildCreditsText(creditsSources), 40f,
                                      creditsSource != null ? creditsSource.fontStyle : FontStyles.Bold);
        SetAlpha(fin, 0f);
        SetAlpha(thanks, 0f);
        SetAlpha(credits, 0f);

        yield return null; // let the canvas scaler lay things out

        // ---- fade in from black onto the menu background ----
        yield return Fade(t => black.color = new Color(0f, 0f, 0f, 1f - t), fadeFromBlack);
        black.gameObject.SetActive(false);

        // ---- "fin." ----
        yield return Fade(t => SetAlpha(fin, t), textFadeIn);
        yield return Wait(finHold);

        // ---- fades into "thank you for playing" ----
        yield return Fade(t => SetAlpha(fin, 1f - t), crossFade * 0.5f);
        yield return Wait(0.3f);
        yield return Fade(t => SetAlpha(thanks, t), crossFade * 0.75f);
        yield return Wait(thanksHold);
        yield return Fade(t => SetAlpha(thanks, 1f - t), textFadeOut);

        // ---- scrolling credits ----
        RectTransform cr = credits.rectTransform;
        float width = Mathf.Min(1500f, canvasRect.rect.width - 120f);
        cr.sizeDelta = new Vector2(width, 100f);
        credits.ForceMeshUpdate();
        float textHeight = credits.GetPreferredValues(credits.text, width, 0f).y;
        cr.sizeDelta = new Vector2(width, textHeight);

        float screenHeight = canvasRect.rect.height;
        float startY = -(screenHeight + textHeight) * 0.5f;
        float endY = (screenHeight + textHeight) * 0.5f;
        cr.anchoredPosition = new Vector2(0f, startY);
        SetAlpha(credits, 1f);

        float y = startY;
        while (y < endY)
        {
            y += creditsSpeed * Time.unscaledDeltaTime;
            cr.anchoredPosition = new Vector2(0f, y);
            yield return null;
        }

        // ---- fade into the main menu ----
        Color tintStart = tintImage.color;
        onFinished?.Invoke();
        yield return Fade(t => tintImage.color = new Color(tintStart.r, tintStart.g, tintStart.b, tintStart.a * (1f - t)), finalFade);

        Destroy(canvasGo);
        Destroy(this);
    }

    // ------------------------------------------------------------------ helpers

    private static string BuildCreditsText(IList<TMP_Text> sources)
    {
        var sb = new System.Text.StringBuilder("<size=150%>Credits</size>");
        if (sources != null)
            foreach (TMP_Text t in sources)
                if (t != null && !string.IsNullOrWhiteSpace(t.text))
                    sb.Append("\n\n\n").Append(t.text.Trim());
        return sb.ToString();
    }

    private static IEnumerator Fade(Action<float> apply, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            apply(k * k * (3f - 2f * k));
            yield return null;
        }
        apply(1f);
    }

    private static IEnumerator Wait(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
    }

    private static Image CreateImage(string name, Transform parent, Color colour)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.GetComponent<Image>();
        img.color = colour;
        img.raycastTarget = true; // keeps the hidden menu from being clicked mid-sequence
        return img;
    }

    private TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, string text, float size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.color = textColour;
        tmp.raycastTarget = false;

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1600f, 300f);
        rt.anchoredPosition = Vector2.zero;
        return tmp;
    }

    private static void SetAlpha(TMP_Text text, float a)
    {
        Color c = text.color;
        c.a = a;
        text.color = c;
    }
}
