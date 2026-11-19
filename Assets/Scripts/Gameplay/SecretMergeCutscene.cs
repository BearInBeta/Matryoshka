using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Secret-level ending: both dolls step into each other, spiral together while hearts in
/// their two colours orbit around them and merge into one doll. The camera slowly pushes in
/// the whole time, then the screen fades to black and the ending sequence takes over.
/// Everything is built at runtime; the only asset used is Resources/SecretHeart (sprite).
/// </summary>
public class SecretMergeCutscene : MonoBehaviour
{
    [Header("Timing")]
    public float meetDuration = 0.3f;      // both dolls step into each other
    public float pauseAfterMeet = 0.35f;
    public float spiralDuration = 3.2f;    // dolls orbit inwards
    public float mergeDuration = 0.7f;     // pop when they become one
    public float afterglowDuration = 2.4f;
    public float fadeOutDuration = 1.8f;   // fade to black at the end

    [Header("Motion")]
    public float meetGap = 0.32f;          // distance left between the dolls after they meet
    public float maxSpinSpeed = 1080f;     // degrees per second around the shared centre
    public float selfSpinSpeed = 720f;     // each doll spinning on its own axis
    public float liftHeight = 0.6f;
    public float spiralBulge = 0.35f;      // dolls swing out a little before closing in

    [Header("Camera")]
    public float zoomedOrthoSize = 1.7f;   // final orthographic size of the push-in
    public float zoomedDistanceFactor = 0.45f; // perspective cameras: fraction of the distance kept

    [Header("Hearts")]
    public int heartsPerColour = 12;
    public float heartRadius = 1.4f;
    public float heartSize = 0.38f;
    public float heartOrbitSpeed = 140f;

    private Camera cam;
    private Sprite heartSprite;
    private readonly List<Heart> hearts = new List<Heart>();

    private class Heart
    {
        public Transform t;
        public SpriteRenderer sr;
        public float angle, radius, height, bobPhase, speedFactor, scale;
        public Color colour;
    }

    public void Play(PlayerController main, PlayerController mirror, Canvas hud, Action onFinished)
    {
        StartCoroutine(Run(main, mirror, hud, onFinished));
    }

    private IEnumerator Run(PlayerController main, PlayerController mirror, Canvas hud, Action onFinished)
    {
        cam = Camera.main;
        heartSprite = LoadHeartSprite();

        Transform a = main.transform;
        Transform b = mirror.transform;
        Color colourA = DollColour(main.gameObject, new Color(0.95f, 0.35f, 0.45f));
        Color colourB = DollColour(mirror.gameObject, new Color(0.45f, 0.6f, 1f));

        Vector3 centre = (a.position + b.position) * 0.5f;
        float baseY = centre.y;

        // Hide the HUD and start the slow push-in for the whole scene.
        if (hud != null)
            StartCoroutine(FadeHud(hud, 0.8f));
        float zoomTime = meetDuration + pauseAfterMeet + spiralDuration + mergeDuration + afterglowDuration + fadeOutDuration;
        StartCoroutine(CameraPushIn(centre + Vector3.up * liftHeight * 0.6f, zoomTime));

        // ---- meet: both dolls take a short step into each other ----
        Vector3 startA = a.position;
        Vector3 startB = b.position;
        Vector3 dirAB = b.position - a.position;
        dirAB.y = 0f;
        float gap = dirAB.magnitude;
        Vector3 unit = gap > 0.0001f ? dirAB / gap : Vector3.forward;
        float stepLen = Mathf.Max(0f, (gap - meetGap) * 0.5f);
        Vector3 endA = startA + unit * stepLen;
        Vector3 endB = startB - unit * stepLen;
        PlaySfx("step");
        float t = 0f;
        while (t < meetDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / meetDuration);
            float e = k * k * (3f - 2f * k);
            float hop = Mathf.Sin(k * Mathf.PI) * 0.15f;
            a.position = Vector3.Lerp(startA, endA, e) + Vector3.up * hop;
            b.position = Vector3.Lerp(startB, endB, e) + Vector3.up * hop;
            yield return null;
        }
        a.position = endA;
        b.position = endB;
        yield return new WaitForSeconds(pauseAfterMeet);

        // ---- spiral in ----
        Vector3 offset = a.position - centre;
        offset.y = 0f;
        float startRadius = offset.magnitude;
        float baseAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;

        SpawnHearts(centre, colourA, colourB);
        PlaySfx("teleport");

        float orbit = 0f;
        t = 0f;
        while (t < spiralDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / spiralDuration);
            float eased = k * k * (3f - 2f * k);

            float spin = Mathf.Lerp(90f, maxSpinSpeed, k * k);
            orbit += spin * Time.deltaTime;

            float radius = Mathf.Lerp(startRadius, 0f, eased) + Mathf.Sin(k * Mathf.PI) * spiralBulge * (1f - k);
            float lift = Mathf.Sin(k * Mathf.PI * 0.5f) * liftHeight;
            float ang = (baseAngle + orbit) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));

            a.position = new Vector3(centre.x, baseY + lift, centre.z) + dir * radius;
            b.position = new Vector3(centre.x, baseY + lift, centre.z) - dir * radius;

            float self = Mathf.Lerp(0f, selfSpinSpeed, k) * Time.deltaTime;
            a.Rotate(Vector3.up, self, Space.World);
            b.Rotate(Vector3.up, -self, Space.World);

            UpdateHearts(centre + Vector3.up * lift, k, 1f + k * 1.5f, Mathf.Clamp01(k * 3f));
            yield return null;
        }

        // ---- merge ----
        PlaySfx("win");
        mirror.gameObject.SetActive(false);
        Vector3 baseScale = a.localScale;
        t = 0f;
        while (t < mergeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / mergeDuration);
            float pop = 1f + Mathf.Sin(k * Mathf.PI) * 0.35f;
            a.localScale = baseScale * pop;
            a.Rotate(Vector3.up, maxSpinSpeed * (1f - k) * Time.deltaTime, Space.World);
            UpdateHearts(centre + Vector3.up * liftHeight, 1f, 2.5f, 1f, 1f + k * 0.8f);
            yield return null;
        }
        a.localScale = baseScale;

        // ---- afterglow: slow spin, hearts drift up and fade ----
        t = 0f;
        while (t < afterglowDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / afterglowDuration);
            a.Rotate(Vector3.up, Mathf.Lerp(180f, 40f, k) * Time.deltaTime, Space.World);
            UpdateHearts(centre + Vector3.up * (liftHeight + k * 1.2f), 1f, Mathf.Lerp(2.5f, 0.6f, k), 1f - k, 1.8f);
            yield return null;
        }

        foreach (Heart h in hearts)
            if (h.t != null) Destroy(h.t.gameObject);
        hearts.Clear();

        // ---- fade to black (the doll keeps turning, the camera keeps pushing in) ----
        Image black = CreateFadeOverlay();
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeOutDuration);
            a.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);
            black.color = new Color(0f, 0f, 0f, k * k * (3f - 2f * k));
            yield return null;
        }
        black.color = Color.black;

        onFinished?.Invoke();
    }

    // ------------------------------------------------------------------ camera / screen

    private IEnumerator CameraPushIn(Vector3 target, float duration)
    {
        if (cam == null) yield break;

        Transform ct = cam.transform;
        Vector3 startPos = ct.position;
        Vector3 fwd = ct.forward;

        // Shift sideways so the target ends up in the centre of the view.
        Vector3 onRay = startPos + fwd * Vector3.Dot(target - startPos, fwd);
        Vector3 shift = target - onRay;

        float startSize = cam.orthographicSize;
        float endSize = Mathf.Min(startSize, zoomedOrthoSize);
        float dist = Vector3.Dot(target - startPos, fwd);
        Vector3 dolly = cam.orthographic ? Vector3.zero : fwd * dist * (1f - zoomedDistanceFactor);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = k * k * k * (k * (6f * k - 15f) + 10f); // smootherstep: slow start, slow finish
            ct.position = startPos + shift * e + dolly * e;
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Lerp(startSize, endSize, e);
            yield return null;
        }
    }

    private static IEnumerator FadeHud(Canvas hud, float duration)
    {
        CanvasGroup group = hud.GetComponent<CanvasGroup>();
        if (group == null) group = hud.gameObject.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        float start = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        group.alpha = 0f;
    }

    private Image CreateFadeOverlay()
    {
        GameObject go = new GameObject("SecretFade", typeof(RectTransform), typeof(Canvas), typeof(Image));
        go.transform.SetParent(transform, false);
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 32000;
        Image img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.raycastTarget = true;
        return img;
    }

    // ------------------------------------------------------------------ hearts

    private void SpawnHearts(Vector3 centre, Color colourA, Color colourB)
    {
        int total = heartsPerColour * 2;
        for (int i = 0; i < total; i++)
        {
            GameObject go = new GameObject("SecretHeart");
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = heartSprite;
            sr.sortingOrder = 50;

            Heart h = new Heart
            {
                t = go.transform,
                sr = sr,
                colour = (i % 2 == 0) ? colourA : colourB,
                angle = 360f * i / total,
                radius = heartRadius * UnityEngine.Random.Range(0.75f, 1.15f),
                height = UnityEngine.Random.Range(-0.2f, 0.9f),
                bobPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                speedFactor = (i % 2 == 0) ? 1f : -1f, // the two colours orbit in opposite directions
                scale = heartSize * UnityEngine.Random.Range(0.8f, 1.2f)
            };
            sr.color = new Color(h.colour.r, h.colour.g, h.colour.b, 0f);
            go.transform.position = centre;
            hearts.Add(h);
        }
    }

    private void UpdateHearts(Vector3 centre, float progress, float speedMul, float alpha, float radiusMul = 1f)
    {
        foreach (Heart h in hearts)
        {
            if (h.t == null) continue;
            h.angle += heartOrbitSpeed * speedMul * h.speedFactor * Time.deltaTime;
            float rad = h.angle * Mathf.Deg2Rad;
            float r = h.radius * radiusMul * Mathf.Lerp(1.2f, 0.85f, progress);
            float bob = Mathf.Sin(Time.time * 3f + h.bobPhase) * 0.12f;
            h.t.position = centre + new Vector3(Mathf.Cos(rad) * r, h.height + bob, Mathf.Sin(rad) * r);

            if (cam != null)
                h.t.rotation = cam.transform.rotation; // billboard

            float pulse = 1f + Mathf.Sin(Time.time * 6f + h.bobPhase) * 0.12f;
            h.t.localScale = Vector3.one * h.scale * pulse;
            h.sr.color = new Color(h.colour.r, h.colour.g, h.colour.b, Mathf.Clamp01(alpha));
        }
    }

    private Sprite LoadHeartSprite()
    {
        Sprite s = Resources.Load<Sprite>("SecretHeart");
        if (s != null)
            return s;

        Texture2D tex = Resources.Load<Texture2D>("SecretHeart");
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));

        Debug.LogWarning("SecretMergeCutscene: Resources/SecretHeart not found.");
        return null;
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Most saturated material colour on the doll's body meshes.</summary>
    private static Color DollColour(GameObject doll, Color fallback)
    {
        Color best = fallback;
        float bestScore = -1f;
        foreach (Renderer r in doll.GetComponentsInChildren<Renderer>(true))
        {
            if (r is SpriteRenderer) continue;
            foreach (Material m in r.sharedMaterials)
            {
                if (m == null) continue;
                Color c;
                if (!TryGetColour(m, out c)) continue;
                Color.RGBToHSV(c, out float h, out float sat, out float val);
                float score = sat * val;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }
        }
        best.a = 1f;
        return best;
    }

    private static bool TryGetColour(Material m, out Color c)
    {
        string[] props = { "_BaseColor", "_AlbedoColor", "_Color" };
        foreach (string p in props)
        {
            if (m.HasProperty(p))
            {
                c = m.GetColor(p);
                return true;
            }
        }
        c = Color.white;
        return false;
    }

    private static void PlaySfx(string clip)
    {
        SFXManager sfx = FindFirstObjectByType<SFXManager>();
        if (sfx != null) sfx.PlayClip(clip);
    }
}
