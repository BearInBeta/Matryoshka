using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ThemeApplier : MonoBehaviour
{
    public ThemeData themeData;
    [SerializeField] Camera maincam;
    [SerializeField] AudioSource themeMusicSource;
    [SerializeField] Image symbol, moustache;
    [ContextMenu("Apply Colors To Materials")]
    public void ApplyColors()
    {
        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text text in allTexts)
        {
            text.color = themeData.textColor;
        }

        MoustacheColorApplier[] mcas = FindObjectsByType<MoustacheColorApplier>(FindObjectsSortMode.None);
        foreach (MoustacheColorApplier mca in mcas)
        {
            mca.color = themeData.stacheColor;
        }

        symbol.color = themeData.textColor;
        moustache.color = themeData.stacheColor;
        themeMusicSource.clip = themeData.themeMusic;
        maincam.backgroundColor = themeData.backgroundColor;
        foreach (var pair in themeData.materialColors)
        {
            if (pair.material == null)
                continue;

            // IMPORTANT: This modifies the actual material asset
            pair.material.color = pair.color;

#if UNITY_EDITOR
            // Ensure the change is saved to the asset
            EditorUtility.SetDirty(pair.material);
#endif
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)   // Object might have been destroyed
                {
                    ApplyColors();
                }
            };
        }
    }
#endif

}
