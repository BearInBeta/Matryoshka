using UnityEngine;
using System.Collections;

public class Block : Item
{
    [Header("Block State")]
    public bool active = true;

    [Header("Deactivate Animation")]
    [SerializeField] float sinkDistance = 0.6f;
    [SerializeField] float sinkDuration = 0.25f;
    [SerializeField] AnimationCurve sinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] Transform spikes;

    private bool isAnimating = false;

    public void Deactivate()
    {
        if (!active || isAnimating)
            return;

        active = false;
        StartCoroutine(SinkAndDisable());
    }

    private IEnumerator SinkAndDisable()
    {
        isAnimating = true;

        Vector3 startPos = spikes.position;
        Vector3 endPos = startPos + Vector3.down * sinkDistance;

        float elapsed = 0f;

        while (elapsed < sinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sinkDuration);
            float eased = sinkCurve.Evaluate(t);

            spikes.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        spikes.position = endPos;



        isAnimating = false;
    }
}
