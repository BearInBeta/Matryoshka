using UnityEngine;

public class ScaleToHeightAndDestroy : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Target Y scale value to reach before destruction")]
    public float targetHeight = 0.2f;

    [Tooltip("Speed at which the object scales")]
    public float scaleSpeed = 1f;

    [Tooltip("If true, object expands. If false, object shrinks.")]
    public bool expand = false;

    [Tooltip("Destroy object when target height is reached")]
    public bool destroyOnComplete = true;

    private Vector3 targetScale;

    void Start()
    {
        targetScale = new Vector3(
            transform.localScale.x,
            targetHeight,
            transform.localScale.z
        );
    }

    void Update()
    {
        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            targetScale,
            scaleSpeed * Time.deltaTime
        );

        // Check if target height has been reached
        if (Mathf.Approximately(transform.localScale.y, targetHeight))
        {
            if (destroyOnComplete)
            {
                Destroy(gameObject);
            }
            else
            {
                enabled = false; // Stop updating if not destroying
            }
        }
    }
}
