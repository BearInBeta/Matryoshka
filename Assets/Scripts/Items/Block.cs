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
    public Direction blockSide = Direction.Left;
    public int maxSize = 1;
    public int id = 0;
    private bool isAnimating = false;

    private void Start()
    {
        if (!active)
        {
            StartCoroutine(SinkAndDisable());
        }
    }
    public bool GetActive(Direction direction, int x, int y, int size)
    {
        if (size > maxSize || active)
        {
            if (Mathf.Abs(this.x - x) <= 1 && Mathf.Abs(this.y - y) <= 1)
            {
                if (direction == blockSide && this.x == x && this.y == y)
                {
                    return true;
                }

                if ((direction == Direction.Up && blockSide == Direction.Down || direction == Direction.Down && blockSide == Direction.Up || direction == Direction.Left && blockSide == Direction.Right || direction == Direction.Right && blockSide == Direction.Left) && (this.x != x || this.y != y))
                {
                    return true;
                }

            }
        }
        return false;
    }
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

        Vector3 startPos = spikes.localPosition;
        Vector3 endPos = startPos + Vector3.down * sinkDistance;

        float elapsed = 0f;

        while (elapsed < sinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sinkDuration);
            float eased = sinkCurve.Evaluate(t);

            spikes.localPosition = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        spikes.localPosition = endPos;



        isAnimating = false;
    }

    public void Activate()
    {
        if (active || isAnimating)
            return;

        active = true;
        StartCoroutine(RiseAndEnable());
    }

    private IEnumerator RiseAndEnable()
    {
        isAnimating = true;

        Vector3 startPos = spikes.localPosition;
        Vector3 endPos = startPos + Vector3.up * sinkDistance;

        float elapsed = 0f;

        while (elapsed < sinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sinkDuration);
            float eased = sinkCurve.Evaluate(t);

            spikes.localPosition = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        spikes.localPosition = endPos;

        isAnimating = false;
    }
}
