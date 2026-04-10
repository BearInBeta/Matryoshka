using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonWobble : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float wobbleAngle = 4f;
    [SerializeField] private float speed = 10f;

    public bool hovering;
    private Vector3 baseScale;
    private Quaternion baseRotation;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        baseScale = target.localScale;
        baseRotation = target.localRotation;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        Vector3 desiredScale = hovering ? baseScale * hoverScale : baseScale;
        target.localScale = Vector3.Lerp(target.localScale, desiredScale, dt * speed);

        Quaternion desiredRot = hovering
            ? Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 20f) * wobbleAngle)
            : baseRotation;

        target.localRotation = Quaternion.Lerp(target.localRotation, desiredRot, dt * speed);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}