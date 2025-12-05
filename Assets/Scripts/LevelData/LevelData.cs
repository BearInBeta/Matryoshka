using UnityEngine;
using System.Collections.Generic;
using static GridManager;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid Settings")]
    public int width = 7;
    public int height = 7;

    [Header("Level Items")]
    public List<ItemSetup> setUpItems = new List<ItemSetup>();

    [Header("Level Items")]
    public string levelName = "New Level";
}
