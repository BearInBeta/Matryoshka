using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
    [SerializeField] Image m_Image;
    [SerializeField] TMP_Text m_Text;
    [SerializeField] Button m_Button;
    [SerializeField] Sprite deleteSprite, addSprite;
    public Color profileColor;
    public string profileName;
    public int profileID;
    public bool deleteDisabled = false;
    MenuController menuController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuController = GameObject.FindGameObjectWithTag("MenuController").GetComponent<MenuController>();
        if(menuController == null)
        {
            Debug.LogError("No Menu Controller");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUp(string profileName, Color profileColor, int profileID, bool deleteDisabled)
    {
        this.profileColor = profileColor;
        this.profileID = profileID;
        this.profileName = profileName;
        this.deleteDisabled = deleteDisabled;
        SetUp();
    }

    private void SetUp()
    {
        m_Image.color = profileColor;
        m_Text.text = profileName;
        if (deleteDisabled)
        {
            m_Button.image.sprite = addSprite;
        }
        else
        {
            m_Button.image.sprite = deleteSprite;
        }
    }

  
    private void DeleteProfile()
    {
        menuController.OpenProfileDeleter(profileID);
    }

    private void CreateProfile()
    {
        menuController.OpenProfileCreator(profileID);
    }
    public void SelectProfile()
    {
        menuController.SelectProfile(profileID);
    }

    public void AddOrDelete()
    {
        if (deleteDisabled)
        {
            CreateProfile();
        }
        else
        {
            DeleteProfile();
        }
    }

    public void AddOrSelect()
    {
        if (deleteDisabled)
        {
            CreateProfile();
        }
        else
        {
            SelectProfile();
        }
    }
}
