using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    List<LevelData> levels;
    [SerializeField] Levels m_levels;
    private LevelData currentLevel;
    public TMP_Text topSizeText, bottomSizeText, stepText, timeText;
    public int startAt = 1;
    [SerializeField] TMP_Text levelName;
    [SerializeField] LevelData testLevel;
    [SerializeField] GameObject passScreen,pauseScreen;
    [SerializeField] private LevelPassedPanelAnimator passedAnimator;
    [SerializeField] GameObject levelsContainer, levelPrefab, levelSelect;
    [Header("Save/Profile")]

    public bool paused = false;
    float levelTimer = 0f;
    bool timerRunning = false;
    int steps = 0;
    bool secretCutscenePlaying = false;

    /// <summary>True while the secret (bonus) level is loaded.</summary>
    public bool IsSecretLevel => m_levels != null && m_levels.secretLevel != null && currentLevel == m_levels.secretLevel;

    void Start()
    {
        levels = m_levels.levels;
        if (testLevel != null)
        {
            LoadLevel(testLevel);
        }
        else
        {
            GameSaveSystem.SetActiveProfile(GameSession.ActiveProfileId);
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
    private void LevelsSetup()
    {
        foreach (Transform child in levelsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < levels.Count; i++)
        {
            GameObject level = Instantiate(levelPrefab, levelsContainer.transform);
            LevelOption levelOption = level.GetComponent<LevelOption>();
            int profileId = GameSaveSystem.GetActiveProfileId();
            if (profileId != -1)
            {
                LevelSaveData lsd = GameSaveSystem.GetLevelData(profileId, i);
                if (lsd != null)
                {
                    levelOption.SetUpButton(true, lsd.stars, i);
                }
                else if (GameSaveSystem.GetNextLevel(profileId, levels.Count) == i)
                {
                    levelOption.SetUpButton(true, 0, i);
                }
                else
                {
                    levelOption.SetUpButton(false, 0, i);
                }
            }
            else
            {
                levelOption.SetUpButton(i == 0, 0, i);
            }
            levelOption.gameManager = this;
        }

        int activeProfile = GameSaveSystem.GetActiveProfileId();
        if (m_levels.secretLevel != null && activeProfile != -1 && GameSaveSystem.HasAllStars(activeProfile, levels.Count))
        {
            GameObject secret = Instantiate(levelPrefab, levelsContainer.transform);
            LevelOption secretOption = secret.GetComponent<LevelOption>();
            secretOption.SetUpSecretButton(levels.Count);
            secretOption.gameManager = this;
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

    /// <summary>Called by a doll that bumps into the other doll. Only reacts on the secret level.</summary>
    public void PlayersTouched(PlayerController a, PlayerController b)
    {
        if (!IsSecretLevel || secretCutscenePlaying)
            return;

        PlayerController main = a.notMain ? b : a;
        PlayerController mirror = a.notMain ? a : b;
        if (main == null || mirror == null || main.notMain || !mirror.notMain)
            return;

        secretCutscenePlaying = true;
        paused = true;            // stops all doll input
        timerRunning = false;

        Canvas hud = levelName != null ? levelName.canvas.rootCanvas : null;
        SecretMergeCutscene cutscene = gameObject.AddComponent<SecretMergeCutscene>();
        cutscene.Play(main, mirror, hud, () =>
        {
            // The cutscene ends on a black screen; the main menu picks up from there.
            GameSession.PlayEnding = true;
            GoToMain();
        });
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
        // Index levels.Count is the secret level (only offered once it is unlocked).
        if (level == levels.Count && m_levels.secretLevel != null)
        {
            LoadLevel(m_levels.secretLevel);
            return;
        }
        LoadLevel(levels[level % levels.Count]);
    }

    private void LoadLevel(LevelData level)
    {
        if(levelSelect.activeInHierarchy)
        {
            levelSelect.GetComponent<UIPanelFader>().FadeOut();
        }
        steps = 0;
        paused = false;
        secretCutscenePlaying = false;
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        passScreen.SetActive(false);
        currentLevel = level;

        levelTimer = 0f;
        
        

        if (m_levels.secretLevel != null && level == m_levels.secretLevel)
            levelName.text = level.levelName;
        else
            levelName.text = "Level " + (levels.IndexOf(level) + 1) + " - " + level.levelName;
        gridManager.LoadLevel(level);
        timerRunning = true;
        UpdateTimeText();
        LevelsSetup();
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
        if (secretCutscenePlaying)
            return;

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
        return deleted;
    }

    public bool SetActiveProfile(int profileId)
    {
        bool success = GameSaveSystem.SetActiveProfile(profileId);


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

    public void OpenLevelSelect()
    {
        levelSelect.SetActive(true);
    }
}