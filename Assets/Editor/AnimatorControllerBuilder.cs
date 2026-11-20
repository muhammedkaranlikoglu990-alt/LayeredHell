#if UNITY_EDITOR
// Generates AC_<id>.controller for rigged models with one state per clip name and NO transitions
// (runtime scripts call animator.CrossFadeInFixedTime(clipName, 0.15f)).
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public static class AnimatorControllerBuilder
    {
        public static readonly string[] HumanoidClipNames =
        {
            "Idle", "Walk", "Run", "Attack", "Attack2", "Hit", "Death", "Scream", "DoorOpen"
        };

        public static readonly string[] FpsClipNames =
        {
            "FPS_Idle", "FPS_Walk", "FPS_Run", "FPS_Fire", "FPS_Reload", "FPS_Draw", "FPS_DoorOpen", "FPS_Melee"
        };

        public static AnimatorController BuildControllerForModel(string modelId, string modelAssetPath, bool isFpsArms)
        {
            const string outDir = "Assets/Art/Animators";
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            string controllerPath = outDir + "/AC_" + modelId + ".controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var rootStateMachine = controller.layers[0].stateMachine;

            // Load embedded AnimationClips from the FBX/GLB asset
            var clipMap = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(modelAssetPath))
            {
                UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelAssetPath);
                for (int i = 0; i < subAssets.Length; i++)
                {
                    var clip = subAssets[i] as AnimationClip;
                    if (clip == null || clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string clean = CleanClipName(clip.name);
                    clipMap[clean] = clip;
                }
            }

            string[] requiredStates = isFpsArms ? FpsClipNames : HumanoidClipNames;
            AnimatorState defaultState = null;

            for (int i = 0; i < requiredStates.Length; i++)
            {
                string stateName = requiredStates[i];
                var state = rootStateMachine.AddState(stateName, new Vector3(260f, i * 55f, 0f));
                AnimationClip clip;
                if (clipMap.TryGetValue(stateName, out clip))
                {
                    state.motion = clip;
                }
                if (i == 0)
                    defaultState = state;
            }

            if (defaultState != null)
                rootStateMachine.defaultState = defaultState;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static string CleanClipName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "Idle";
            int pipe = raw.LastIndexOf('|');
            if (pipe >= 0 && pipe + 1 < raw.Length)
                raw = raw.Substring(pipe + 1);
            return raw.Trim();
        }
    }
}
#endif
