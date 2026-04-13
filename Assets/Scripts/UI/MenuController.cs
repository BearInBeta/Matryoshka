using System.Collections;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject profilePrefab, profileContainer, levelPrefab, levelsContainer;
    [SerializeField] Color[] profileColors;
    [SerializeField] GameObject options, profileSelect, levelSelect, title, profile, profileCreator, profileDeleter, gameQuitter;
    [SerializeField] ProfileController profileController;
    [SerializeField] TMP_InputField m_name;
    [SerializeField] TMP_Text areYouSure;
    [SerializeField] int NumberOfLevels;
    [SerializeField] GameObject Player, Particles;
    [SerializeField] float waitForSeconds = 0.5f;
    private int profileIdToBeCreated;
    private int profileIdToBeDeleted;
    private int levelToStart = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;
        options.SetActive(true);
        title.SetActive(true);
        profile.SetActive(true);

        ProfileSetup();
        ProfileSelectSetup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        int nextLevel = GameSaveSystem.GetNextLevel(GameSaveSystem.GetActiveProfileId(), NumberOfLevels);
        if (nextLevel < NumberOfLevels && nextLevel > -1)
            GoToLevel(nextLevel);
        else
            GoToLevel(0);

    }

    public void GoToLevel(int levelIndex)
    {
        if (GameSaveSystem.GetActiveProfileId() == -1)
        {
            
            DisableMenu(options);
            DisableMenu(levelSelect);
            OpenProfileCreator(0);
            levelToStart = levelIndex;
        }
        else
        {
            GameSession.ActiveProfileId = GameSaveSystem.GetActiveProfileId();
            GameSession.LevelToLoad = levelIndex;
            StartCoroutine(LoadSceneCoroutine());
        }
    }

    IEnumerator LoadSceneCoroutine()
    {
        FindFirstObjectByType<SFXManager>().PlayClip("success");
        DisableMenu(options);
        DisableMenu(levelSelect);
        DisableMenu(title);
        DisableMenu(profile);
        DisableMenu(levelSelect);
        DisableMenu(profileSelect);
        DisableMenu(profileCreator);
        DisableMenu(profileDeleter);
        yield return new WaitForSeconds(waitForSeconds);
        Instantiate(Particles, Player.transform.position, Quaternion.identity);
        Player.SetActive(false);
        FindFirstObjectByType<SFXManager>().PlayClip("win");
        yield return new WaitForSeconds(waitForSeconds);
        SceneManager.LoadScene("Game");

    }
    private void ProfileSetup()
    {
        if(GameSaveSystem.GetActiveProfileId() != -1)
        {
            profileController.SetUp(GameSaveSystem.GetProfile(GameSaveSystem.GetActiveProfileId()).profileName, profileColors[GameSaveSystem.GetActiveProfileId()], GameSaveSystem.GetActiveProfileId(), true);
            LevelsSetup();
            return;
        }
        for (int i = 0; i < GameSaveSystem.MaxProfiles; i++)
        {
            if (GameSaveSystem.HasProfile(i))
            {
                profileController.SetUp(GameSaveSystem.GetProfile(i).profileName, profileColors[i % profileColors.Length], i, true);
                GameSaveSystem.SetActiveProfile(i);
                LevelsSetup();
                return;
            }
        }

        profileController.SetUp("New Profile", profileColors[0], 0, true);
        LevelsSetup();

    }
    private void ProfileSelectSetup()
    {
        foreach (Transform child in profileContainer.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < GameSaveSystem.MaxProfiles; i++)
        {
            GameObject profile = Instantiate(profilePrefab, profileContainer.transform);
            ProfileController profileController = profile.GetComponent<ProfileController>();
            if(GameSaveSystem.HasProfile(i))
            {
                profileController.SetUp(GameSaveSystem.GetProfile(i).profileName, profileColors[i % profileColors.Length], i, false);
            }
            else
            {
                profileController.SetUp("New Profile", profileColors[i % profileColors.Length], i, true);
            }
        }
    }
    private void LevelsSetup()
    {
        foreach (Transform child in levelsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < NumberOfLevels; i++)
        {
            GameObject level = Instantiate(levelPrefab, levelsContainer.transform);
            LevelOption levelOption = level.GetComponent<LevelOption>();
            int profileId = GameSaveSystem.GetActiveProfileId();
            if (profileId != -1)
            {
                LevelSaveData lsd = GameSaveSystem.GetLevelData(profileId, i);
                if(lsd != null)
                {
                    levelOption.SetUpButton(true, lsd.stars, i);
                }else if(GameSaveSystem.GetNextLevel(profileId, NumberOfLevels) == i)
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

            levelOption.menuController = this;
        }
    }
    public void EnableMenu(GameObject menu)
    {
        if (menu.activeInHierarchy)
            return;
        menu.SetActive(true);
        FindFirstObjectByType<SFXManager>().PlayClip("star");
    }

    public void DisableMenu(GameObject menu)
    {
        if (!menu.activeInHierarchy)
            return;
        if(menu.GetComponent<UIPanelFader>() != null)
        {
            menu.GetComponent<UIPanelFader>().FadeOut();
        }
        else
        {
            menu.SetActive(false);
        }

        
        levelToStart = -1;
    }

    public void OpenProfileCreator(int profileIdToBeCreated)
    {
        m_name.text = "";
        this.profileIdToBeCreated = profileIdToBeCreated;
        DisableMenu(profileSelect);
        EnableMenu(profileCreator);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void OpenProfileDeleter(int profileIdToBeDeleted)
    {
        areYouSure.text = "Delete profile " + '"' + GameSaveSystem.GetProfile(profileIdToBeDeleted).profileName + '"';
        this.profileIdToBeDeleted = profileIdToBeDeleted;
        DisableMenu(profileSelect);
        EnableMenu(profileDeleter);
    }
    public void CreateNewProfile()
    {
        if (m_name.text.Equals(""))
        {
            m_name.placeholder.color = Color.red;
            FindFirstObjectByType<SFXManager>().PlayClip("error");
            return;
        }
        int newProfile = GameSaveSystem.CreateProfile(m_name.text, profileIdToBeCreated);
        
        SelectProfile(newProfile);
        if (levelToStart != -1)
        {
            GoToLevel(levelToStart);
            return;
        }
        ProfileSelectSetup();
        DisableMenu(profileCreator);
        
    }

    public void SelectProfile(int profileID)
    {
        GameSaveSystem.SetActiveProfile(profileID);
        ProfileSetup();
        DisableMenu(profileSelect);
        EnableMenu(options);
    }

    public void DeleteProfile()
    {
        GameSaveSystem.DeleteProfile(profileIdToBeDeleted);
        ProfileSetup();
        ProfileSelectSetup();
        DisableMenu(profileDeleter);
        EnableMenu(profileSelect);
    }


}
