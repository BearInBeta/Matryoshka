using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelOption : MonoBehaviour
{
    [SerializeField] TMP_Text levelNumber;
    [SerializeField] GameObject star1, star2, star3;
    [SerializeField] GameObject stars, lockpad;
    [SerializeField] Button button;
    private int levelIndex;
    MenuController menuController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuController = GameObject.FindGameObjectWithTag("MenuController").GetComponent<MenuController>();
        if (menuController == null)
        {
            Debug.LogError("No Menu Controller");
        }
    }

    // Update is called once per frame
    void Update()
    {
         
    }
    public void GoToLevel()
    {
        
        menuController.GoToLevel(levelIndex);
    }
    public void SetUpButton(bool open, int starsNumber, int level)
    {
        levelIndex = level;
        star1.SetActive(false); star2.SetActive(false); star3.SetActive(false);
        levelNumber.text = (level + 1) + "";
        if(!open)
        {
            stars.SetActive(false);
            lockpad.SetActive(true);
            button.interactable = false;
        }
        else
        {
            stars.SetActive(true);
            lockpad.SetActive(false);
            
            switch (starsNumber)
            {
                case 1: star1.SetActive(true); break;
                case 2: star1.SetActive(true); star2.SetActive(true); break;
                case 3: star1.SetActive(true); star2.SetActive(true); star3.SetActive(true); break;
                default: break;
            }
        }
    }
}
