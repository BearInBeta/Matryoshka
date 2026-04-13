using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    [SerializeField] Sprite imageOff, imageOn;
    [SerializeField] Image image;
    [SerializeField] AudioSource audioSource;
    [SerializeField] string key;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!PlayerPrefs.HasKey(key) || PlayerPrefs.GetInt(key) == 1)
        {
            if(image != null && image.sprite != imageOn)
            {
                image.sprite = imageOn;
            }

            if(audioSource != null)
            {
                audioSource.volume = 1;
            }
        }
        else
        {
            if (image != null && image.sprite != imageOff)
            {
                image.sprite = imageOff;
            }

            if (audioSource != null)
            {
                audioSource.volume = 0;
            }
        }
    }

    public void ToggleSetting()
    {
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, Mathf.Abs(PlayerPrefs.GetInt(key) - 1));
        }
        else
        {
            PlayerPrefs.SetInt(key, 0);

        }
    }
}
