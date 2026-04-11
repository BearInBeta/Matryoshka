using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelOption : MonoBehaviour
{
    [SerializeField] TMP_Text levelNumber;
    [SerializeField] GameObject star1, star2, star3;
    [SerializeField] GameObject stars, lockpad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUpButton(bool passed, int starsNumber, int level)
    {
        levelNumber.text = level + "";
        if(!passed)
        {
            stars.SetActive(false);
            lockpad.SetActive(true);
        }
        else
        {
            star1.SetActive(false); star2.SetActive(false); star3.SetActive(false);
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
