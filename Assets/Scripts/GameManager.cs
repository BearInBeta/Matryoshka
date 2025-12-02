using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public List<LevelData> levels;
    private LevelData currentLevel;

    void Start()
    {
        LoadLevel(0);
    }
    public void NextLevel()
    {
        int currentLevelNumber = levels.IndexOf(currentLevel);
        if (currentLevelNumber != levels.Count)
            LoadLevel(currentLevelNumber + 1);
        else
            Debug.Log("YOU BEAT THE GAME");
    }
    public void LoadLevel(int level)
    {
        LoadLevel(levels[level]);
    }
    private void LoadLevel(LevelData level)
    {
        currentLevel = level;
        gridManager.LoadLevel(level);
    }

    public void ResetLevel()
    {
        if (currentLevel != null)
        {
            gridManager.LoadLevel(currentLevel);
        }
    }
}
