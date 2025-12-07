
public class Teleporter : Item
{
    public bool active = true;
    public int id = 0;
    private void Update()
    {
        if (!active)
        {
            GetComponentInChildren<ContinuousRotation>().enabled = false;
        }
        else
        {
            GetComponentInChildren<ContinuousRotation>().enabled = true;
        }
    }
}
