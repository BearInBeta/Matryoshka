using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid Settings")]
    public int width = 7;
    public int height = 7;

    [Header("Level Items")]
    public List<ItemSetup> setUpItems = new List<ItemSetup>();

    [Header("Level Data")]
    public string levelName = "New Level";
    public int minTime;
    public int minSteps;
}
