using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine.Rendering;

#if LLG_USE_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Lith.LiquidGlass
{
    [InitializeOnLoad]
    public static class PipelineDefineSetter
    {
        const string urpDefine = "LLG_USE_URP";
        const string hdrpDefine = "LLG_USE_HDRP";
        const string compabilityModeDefine = "LLG_COMPABILITY_MODE";
        private static string lastPipelineType = "";
        private static bool lastCompabilityMode
        {
            get => EditorPrefs.GetBool("LastCompabilityMode", false);
            set => EditorPrefs.SetBool("LastCompabilityMode", value);
        }

        static PipelineDefineSetter()
        {
            UpdateDefines();
            EditorApplication.update += CheckPipelineChange;
        }

        private static void CheckPipelineChange()
        {
            var asset = GraphicsSettings.defaultRenderPipeline;
            string type = asset ? asset.GetType().ToString() : "Builtin";


            if (type != lastPipelineType || lastCompabilityMode != IsCompabilityModeEnabled())
            {
                lastPipelineType = type;
                UpdateDefines();
            }
        }

        [MenuItem("Tools/Lith/Update RenderPipeline Defines")]
        public static void UpdateDefines()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            string defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var oldDefines = defines;

            bool hasURP = false;
            bool hasHDRP = false;

            var asset = GraphicsSettings.defaultRenderPipeline;
            if (asset != null)
            {
                var type = asset.GetType().ToString();
                if (type.Contains("UniversalRenderPipelineAsset"))
                    hasURP = true;
                else if (type.Contains("HDRenderPipelineAsset"))
                    hasHDRP = true;
            }

            defines = defines.Replace(urpDefine, "")
                             .Replace(hdrpDefine, "")
                             .Replace(compabilityModeDefine, "")
                             .Replace(";;", ";")
                             .Trim(';', ' ');

            if (hasURP)
                defines = AppendDefine(defines, urpDefine);
            else if (hasHDRP)
                defines = AppendDefine(defines, hdrpDefine);


            lastCompabilityMode = IsCompabilityModeEnabled();
            if (lastCompabilityMode)
                defines = AppendDefine(defines, compabilityModeDefine);

            PlayerSettings.SetScriptingDefineSymbols(namedTarget, defines);

            if (oldDefines != defines)
                CompilationPipeline.RequestScriptCompilation();
        }

        private static string AppendDefine(string current, string define)
        {
            if (string.IsNullOrEmpty(current))
                return define;
            if (!current.Contains(define))
                return current + ";" + define;
            return current;
        }
        public static bool IsCompabilityModeEnabled()
        {
#if LLG_USE_URP
            if (GetActiveUniversalRendererData() == null)
                return lastCompabilityMode;
            else
                return GetActiveUniversalRendererData().settings.compabilityModeEnabled;
#else
            return false;
#endif
        }

#if LLG_USE_URP
        public static LiquidGlassRenderFeature GetActiveUniversalRendererData()
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset
                      ?? UnityEngine.QualitySettings.renderPipeline as UniversalRenderPipelineAsset;

            if (urp == null) return null;

            var so = new SerializedObject(urp);
            int defaultIndex = so.FindProperty("m_DefaultRendererIndex").intValue;
            var listProp = so.FindProperty("m_RendererDataList");
            if (listProp == null || defaultIndex < 0 || defaultIndex >= listProp.arraySize) return null;

            var rendererData = listProp.GetArrayElementAtIndex(defaultIndex).objectReferenceValue as UniversalRendererData;
            LiquidGlassRenderFeature liquidGlassRenderFeature;
            rendererData.TryGetRendererFeature(out liquidGlassRenderFeature);
            return liquidGlassRenderFeature;
        }
#endif
    }
}