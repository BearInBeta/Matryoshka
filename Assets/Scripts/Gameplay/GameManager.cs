using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public List<LevelData> levels;
    private LevelData currentLevel;
    public TMP_Text topSizeText, bottomSizeText, stepText, timeText;
    public int startAt = 1;
    [SerializeField] TMP_Text levelName;
    [SerializeField] LevelData testLevel;
    [SerializeField] GameObject passScreen,pauseScreen;
    [SerializeField] private LevelPassedPanelAnimator passedAnimator;

    [Header("Save/Profile")]
    [SerializeField] private int activeProfileId = -1;

    public bool paused = false;
    float levelTimer = 0f;
    bool timerRunning = false;
    int steps = 0;

    void Start()
    {
        if(testLevel != null)
        {
            LoadLevel(testLevel);
        }
        else
        {
            GameSaveSystem.SetActiveProfile(GameSession.ActiveProfileId);
            activeProfileId = GameSaveSystem.GetActiveProfileId();
            LoadLevel(GameSession.LevelToLoad);
        }

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
        paused = true;
        timerRunning = false;
        int levelIndex = levels.IndexOf(currentLevel);
        if (levelIndex >= 0)
            GameSaveSystem.RecordLevelPassed(levelIndex, currentLevel, levelTimer, steps);

        foreach (ButtonWobble pbw in passScreen.GetComponentsInChildren<ButtonWobble>())
        {
            pbw.hovering = false;
        }
        passedAnimator.Play(currentLevel, levelTimer, steps);
    }

    public void UpdateSizeText(int topSize, int bottomSize)
    {
        topSizeText.text = topSize + "";
        bottomSizeText.text = bottomSize + "";
    }

    public void UpdateStepText(int steps)
    {
        this.steps = steps;
        if (steps <= 999)
            stepText.text = steps + "";
        else
            stepText.text = 999 + "+";
    }

    void UpdateTimeText()
    {
        int seconds = Mathf.FloorToInt(levelTimer);

        timeText.text = $"{seconds:00}";
    }
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TogglePause();
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
        steps = 0;
        paused = false;
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        passScreen.SetActive(false);
        currentLevel = level;

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

        paused = !paused;
        if (paused)
        {
            FindFirstObjectByType<SFXManager>().PlayClip("pause");
            Time.timeScale = 0f;
            pauseScreen.SetActive(true);
            pauseScreen.GetComponent<PauseMenuAnimator>().Show();
        }
        else
        {
            FindFirstObjectByType<SFXManager>().PlayClip("unpause");
            Time.timeScale = 1f;
            foreach (ButtonWobble pbw in pauseScreen.GetComponentsInChildren<ButtonWobble>())
            {
                pbw.hovering = false;
            }
            pauseScreen.GetComponent<PauseMenuAnimator>().Hide();
            
        }

    }

    public void GoToMain()
    {
        SceneManager.LoadScene("Main");
    }

    // ---------- Profile methods ----------



    public bool DeleteProfile(int profileId)
    {
        bool deleted = GameSaveSystem.DeleteProfile(profileId);
        activeProfileId = GameSaveSystem.GetActiveProfileId();
        return deleted;
    }

    public bool SetActiveProfile(int profileId)
    {
        bool success = GameSaveSystem.SetActiveProfile(profileId);
        if (success)
            activeProfileId = profileId;

        return success;
    }

    public List<SaveProfile> GetProfiles()
    {
        return GameSaveSystem.GetProfiles();
    }

    public int GetCurrentStars()
    {
        return GameSaveSystem.CalculateStars(currentLevel, levelTimer, steps);
    }
}