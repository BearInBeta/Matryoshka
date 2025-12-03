using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public List<LevelData> levels;
    private LevelData currentLevel;
    public TMP_Text topSizeText, bottomSizeText;

    void Start()
    {
        LoadLevel(0);
    }
    void Update()
    {

    }

    public void UpdateSizeText(int topSize, int bottomSize)
    {
        topSizeText.text = topSize + "";
        bottomSizeText.text = bottomSize + "";

    }
    // Called by PlayerInput → Testing → Reset UnityEvent
    public void OnReset(InputAction.CallbackContext context)
    {
        if (!context.performed) return;   // 👈 filter out started/canceled
        ResetLevel();
    }

    // Called by PlayerInput → Testing → Next UnityEvent
    public void OnNext(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        NextLevel();
    }

    // Called by PlayerInput → Testing → Previous UnityEvent
    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        PreviousLevel();
    }
    public void PreviousLevel()
    {

        if (levels.Count == 0)
            return;

        if (levels.IndexOf(currentLevel) > 0)
        {
            LoadLevel(levels.IndexOf(currentLevel) - 1);
        }
        else
        {
            Debug.Log("Already at first level");
        }
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
