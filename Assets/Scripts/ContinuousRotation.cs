using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z,
        Custom
    }

    [Header("Rotation Settings")]
    public RotationAxis axis = RotationAxis.Y;
    public Vector3 customAxis = Vector3.up;
    public float rotationSpeed = 90f; // Degrees per second
    public Space rotationSpace = Space.Self;

    void Update()
    {
        Vector3 selectedAxis = GetSelectedAxis();
        transform.Rotate(selectedAxis, rotationSpeed * Time.deltaTime, rotationSpace);
    }

    private Vector3 GetSelectedAxis()
    {
        switch (axis)
        {
            case RotationAxis.X:
                return Vector3.right;
            case RotationAxis.Y:
                return Vector3.up;
            case RotationAxis.Z:
                return Vector3.forward;
            case RotationAxis.Custom:
                return customAxis.normalized;
            default:
                return Vector3.up;
        }
    }
}
