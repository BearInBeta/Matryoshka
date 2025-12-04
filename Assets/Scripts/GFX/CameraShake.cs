using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Base Shake")]
    [SerializeField] float baseShakeStrength = 0.04f;
    [SerializeField] float baseShakeDuration = 0.12f;

    [Header("Scaling")]
    [SerializeField] float sizeShakeMultiplier = 0.015f; // per extra size
    [SerializeField] float jumpMultiplier = 1.6f;        // launcher boost

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
    }

    public void Shake(int totalSize, bool isJump)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        // No shake at 1 + 1
        int excessSize = Mathf.Max(0, totalSize - 2);

        float strength =
            baseShakeStrength +
            excessSize * sizeShakeMultiplier;

        if (isJump)
            strength *= jumpMultiplier;

        shakeRoutine = StartCoroutine(ShakeRoutine(strength));
    }

    private IEnumerator ShakeRoutine(float strength)
    {
        float elapsed = 0f;

        while (elapsed < baseShakeDuration)
        {
            elapsed += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
