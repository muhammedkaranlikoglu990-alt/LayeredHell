#if UNITY_EDITOR
// Creates mobile-optimized UniversalRenderPipelineAsset and UniversalRendererData under Assets/Settings/.
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LayeredHell.EditorTools
{
    public static class URPAssetBuilder
    {
        [MenuItem("Layered Hell/5. Build URP Mobile Pipeline Assets")]
        public static UniversalRenderPipelineAsset BuildURPAssets()
        {
            const string settingsDir = "Assets/Settings";
            if (!Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);

            string rendererPath = settingsDir + "/LayeredHell_MobileRenderer.asset";
            string pipelinePath = settingsDir + "/LayeredHell_URP_Mobile.asset";

            UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }

            UniversalRenderPipelineAsset pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, pipelinePath);
            }

            pipelineAsset.supportsHDR = true;
            pipelineAsset.msaaSampleCount = 2;
            pipelineAsset.renderScale = 1.0f;
            pipelineAsset.shadowDistance = 32f;
            pipelineAsset.shadowCascadeCount = 2;
            pipelineAsset.supportsCameraDepthTexture = false;
            pipelineAsset.supportsCameraOpaqueTexture = false;
            pipelineAsset.useSRPBatcher = true;

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;

            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(pipelineAsset);
            AssetDatabase.SaveAssets();

            Debug.Log("[LayeredHell] Configured URP Mobile Pipeline Asset at " + pipelinePath);
            return pipelineAsset;
        }
    }
}
#endif
