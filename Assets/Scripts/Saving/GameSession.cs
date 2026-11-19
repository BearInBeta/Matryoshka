using UnityEngine;

public static class GameSession
{
    public static int ActiveProfileId;
    public static int LevelToLoad;
    /// <summary>Set by the secret level; the main menu plays the ending sequence once.</summary>
    public static bool PlayEnding;
}
