using UnityEngine;

public class Launcher : Item
{
    [Header("Launch Distances")]
    public int upDistance = 1;
    public int downDistance = 1;
    public int leftDistance = 1;
    public int rightDistance = 1;

    public int GetDistance(int dx, int dy)
    {
        if (dy == 0 && dx > 0)        // Up (your iso mapping)
            return upDistance;
        else if (dy == 0 && dx < 0)   // Down
            return downDistance;
        else if (dy > 0 && dx == 0)   // Right
            return rightDistance;
        else if (dy < 0 && dx == 0)   // Left
            return leftDistance;

        return 1;
    }
}
