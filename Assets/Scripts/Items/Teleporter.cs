
public class Teleporter : Item
{
    public bool active = true;

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
