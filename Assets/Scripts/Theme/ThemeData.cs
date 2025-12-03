using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "Scriptable Objects/ThemeData")]
public class ThemeData : ScriptableObject
{
    [System.Serializable]
    public class MaterialColorPair
    {
        public Material material;
        public Color color = Color.white;
    }
    public Color backgroundColor, stacheColor, textColor, flashColor;
    public AudioClip themeMusic;
    public List<MaterialColorPair> materialColors = new List<MaterialColorPair>();

}
