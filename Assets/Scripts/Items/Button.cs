using UnityEngine;
using System.Collections;

public class Button : Item
{
    public enum ButtonType
    {
        DeactivateBlocks,
        ActivateTeleporters
    }

    [Header("Button State")]
    public bool active = true;
    public ButtonType type;
    public int size = 1;

    [Header("Button Visual")]
    public Transform button;
    public float pressedHeight = -0.94f;
    public float unpressedHeight = -0.9f;

    [Header("Failed Press Animation")]
    [SerializeField] float failedPressDuration = 0.08f;

    private Coroutine failedPressRoutine;

    public void Pressed()
    {
        active = false;

        // Snap to pressed height
        button.localPosition = new Vector3(
            button.localPosition.x,
            pressedHeight,
            button.localPosition.z
        );

        // Darken
        button.gameObject.GetComponent<Renderer>().material.color *= 0.7f;
    }

    public void Unpressed()
    {
        active = true;

        button.localPosition = new Vector3(
            button.localPosition.x,
            unpressedHeight,
            button.localPosition.z
        );

        button.gameObject.GetComponent<Renderer>().material.color /= 0.7f;
    }

    // ✅ CALL THIS WHEN THE PLAYER IS TOO SMALL TO PRESS
    public void FailedPress()
    {
        if (!active || button == null)
            return;

        if (failedPressRoutine != null)
            StopCoroutine(failedPressRoutine);

        failedPressRoutine = StartCoroutine(FailedPressRoutine());
    }

    private IEnumerator FailedPressRoutine()
    {
        float startY = button.localPosition.y;

        // Move 1/3 of the way toward pressed height
        float targetY = Mathf.Lerp(startY, pressedHeight, 1f / 3f);

        Vector3 start = button.localPosition;
        Vector3 mid = new Vector3(start.x, targetY, start.z);

        float t = 0f;

        // Quick dip
        while (t < failedPressDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / failedPressDuration);
            button.localPosition = Vector3.Lerp(start, mid, lerp);
            yield return null;
        }

        // Snap straight back to unpressed height
        button.localPosition = new Vector3(
            start.x,
            unpressedHeight,
            start.z
        );

        failedPressRoutine = null;
        FindFirstObjectByType<SFXManager>().PlayClip("stuck");

    }
}
