using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public int x;
    public int y;

    // Called when the item is added to the grid
    public virtual void Initialize(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
    }
}
