using UnityEngine;

public class LevelSeed : MonoBehaviour
{
    public static int Seed;

    [Header("If zero, a random seed will be generated")]
    public int manualSeed = 0;

    void Awake()
    {
        if (manualSeed != 0)
            Seed = manualSeed;
        else
            Seed = Random.Range(int.MinValue, int.MaxValue);

        Random.InitState(Seed);
    }
}
