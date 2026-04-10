using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public List<LevelData> levels;
    private LevelData currentLevel;
    public TMP_Text topSizeText, bottomSizeText, stepText, timeText;
    public int startAt = 0;
    [SerializeField] TMP_Text levelName;
    [SerializeField] LevelData testLevel;
    [SerializeField] GameObject passScreen,pauseScreen;

    bool passed = false;
    float levelTimer = 0f;
    bool timerRunning = false;

    void Start()
    {
        if (testLevel == null)
            LoadLevel(startAt - 1);
        else
            LoadLevel(testLevel);
    }

    void Update()
    {
        if (timerRunning)
        {
            levelTimer += Time.deltaTime;
            UpdateTimeText();
        }
    }

    public void PassLevel()
    {
        passScreen.SetActive(true);
        passed = true;
        timerRunning = false;
        UpdateTimeText();
    }

    public void UpdateSizeText(int topSize, int bottomSize)
    {
        topSizeText.text = topSize + "";
        bottomSizeText.text = bottomSize + "";
    }

    public void UpdateStepText(int steps)
    {
        stepText.text = steps + "";
    }

    void UpdateTimeText()
    {
        int minutes = Mathf.FloorToInt(levelTimer / 60f);
        int seconds = Mathf.FloorToInt(levelTimer % 60f);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // Called by PlayerInput → Testing → Reset UnityEvent
    public void OnReset(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
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
        LoadLevel(levels[level % levels.Count]);
    }

    private void LoadLevel(LevelData level)
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        passScreen.SetActive(false);
        currentLevel = level;
        passed = false;

        levelTimer = 0f;
        
        

        levelName.text = "Level " + (levels.IndexOf(level) + 1) + " - " + level.levelName;
        gridManager.LoadLevel(level);
        timerRunning = true;
        UpdateTimeText();
    }

    public void ResetLevel()
    {
        if (currentLevel != null)
        {
            levelTimer = 0f;
            timerRunning = true;
            UpdateTimeText();

            LoadLevel(currentLevel);
        }
    }
    public void TogglePause()
    {
        pauseScreen.SetActive(!pauseScreen.activeInHierarchy);
        Time.timeScale = Mathf.Abs(Time.timeScale - 1);
    }
}