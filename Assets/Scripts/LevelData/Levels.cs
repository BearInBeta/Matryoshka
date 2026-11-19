using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Levels", menuName = "Scriptable Objects/Levels")]
public class Levels : ScriptableObject
{
    public List<LevelData> levels;

    [Tooltip("Bonus level that unlocks once every level above has 3 stars.")]
    public LevelData secretLevel;
}
