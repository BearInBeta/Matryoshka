using System.Collections;
using UnityEngine;

public class NestingStation : Item
{
    public bool active = true;
    private Vector3[] originalScales;
    public int id = 0;
    public GameObject[] rings;

    [Header("Pulse Motion")]
    public float moveAmount = 0.25f;
    public float pulseDuration = 0.6f;
    public float delayBetweenRings = 0.12f;

    private Coroutine loopRoutine;
    private Coroutine hideRoutine;

    private void Awake()
    {
        originalScales = new Vector3[rings.Length];

        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
                originalScales[i] = rings[i].transform.localScale;
        }
    }
    private void OnEnable()
    {
        if (active)
            StartEffect();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetActive(bool value)
    {
        if (active == value)
            return;

        active = value;

        if (active)
            StartEffect();
        else
            StopEffect();
    }

    private void StartEffect()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        foreach (GameObject ring in rings)
        {
            if (ring != null)
                ring.SetActive(true);
        }

        if (loopRoutine != null)
            StopCoroutine(loopRoutine);

        loopRoutine = StartCoroutine(PulseLoop());
    }

    private void StopEffect()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);

        hideRoutine = StartCoroutine(HideRings());
    }

    private IEnumerator PulseLoop()
    {
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
                StartCoroutine(PulseRingLoop(rings[i], i * delayBetweenRings));
        }

        while (active)
            yield return null;
    }

    private IEnumerator PulseRingLoop(GameObject ring, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        Transform t = ring.transform;
        Vector3 basePosition = t.localPosition;
        Vector3 movedPosition = basePosition + Vector3.up * moveAmount;

        while (active)
        {
            float elapsed = 0f;

            while (elapsed < pulseDuration && active)
            {
                elapsed += Time.deltaTime;

                float p = Mathf.Clamp01(elapsed / pulseDuration);
                float eased = 1f - Mathf.Pow(1f - p, 3f);

                t.localPosition = Vector3.Lerp(basePosition, movedPosition, eased);

                yield return null;
            }

            // This ring snaps back immediately when IT reaches the top.
            t.localPosition = basePosition;
        }
    }

    private IEnumerator HideRings()
    {
        float duration = 0.35f;
        float elapsed = 0f;

        Vector3[] originalScales = new Vector3[rings.Length];

        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
                originalScales[i] = rings[i].transform.localScale;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;

            for (int i = 0; i < rings.Length; i++)
            {
                if (rings[i] == null)
                    continue;

                rings[i].transform.localScale =
                    Vector3.Lerp(originalScales[i], Vector3.zero, p);
            }

            yield return null;
        }

        foreach (GameObject ring in rings)
        {
            if (ring == null)
                continue;

            ring.SetActive(false);

            int index = System.Array.IndexOf(rings, ring);
            ring.transform.localScale = originalScales[index];
        }
    }
}