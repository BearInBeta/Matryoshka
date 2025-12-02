using UnityEngine;

public enum DollPieceType
{
    Top,
    Bottom
}

public class DollPiece : Item
{
    [Header("Doll Piece Properties")]
    public int size; // must be >= 2
    public DollPieceType type;

    public void AttachTo(Transform parent, Vector3 localOffset)
    {
        transform.SetParent(parent);
        transform.localPosition = localOffset;
        if (type == DollPieceType.Top)
            transform.localRotation = Quaternion.Euler(0, 90, 180);
        else
            transform.localRotation = Quaternion.Euler(0, 90, 0);

    }

}
