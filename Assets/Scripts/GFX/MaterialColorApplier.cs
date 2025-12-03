using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MaterialColorApplier : MonoBehaviour
{
    [System.Serializable]
    public class MaterialColorPair
    {
        public Material material;
        public Color color = Color.white;
    }
    public Color backgroundColor = Color.white;
    public List<MaterialColorPair> materialColors = new List<MaterialColorPair>();
    [SerializeField] Camera maincam;
    [ContextMenu("Apply Colors To Materials")]
    public void ApplyColors()
    {
        maincam.backgroundColor = backgroundColor;
        foreach (var pair in materialColors)
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

    // Optional: Auto-apply when values change in the Inspector (Editor only)
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ApplyColors();
        }
#endif
    }
}
