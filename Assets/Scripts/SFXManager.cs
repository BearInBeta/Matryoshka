using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [System.Serializable]
    public class NamedClip
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Sound Effects")]
    [SerializeField] private List<NamedClip> clips = new List<NamedClip>();

    private Dictionary<string, AudioClip> clipMap;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        clipMap = new Dictionary<string, AudioClip>();

        foreach (var entry in clips)
        {
            if (!string.IsNullOrEmpty(entry.name) && entry.clip != null)
            {
                if (!clipMap.ContainsKey(entry.name))
                {
                    clipMap.Add(entry.name, entry.clip);
                }
                else
                {
                    Debug.LogWarning($"Duplicate SFX name detected: {entry.name}");
                }
            }
        }
    }

    public void PlayClip(string name)
    {
        if (clipMap.TryGetValue(name, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"SFXManager: No AudioClip found with name '{name}'");
        }
    }
}
