// Ensures GameBootstrap, GameRenderingController, and ProceduralSkyDarkener exist even if a scene is played directly in Editor.
using UnityEngine;

namespace LayeredHell
{
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapped()
        {
            if (GameManager.Instance == null && Object.FindFirstObjectByType<GameBootstrap>() == null)
            {
                var bootGo = new GameObject("AutoGameBootstrap");
                bootGo.AddComponent<GameBootstrap>();
            }

            if (GameRenderingController.Instance == null)
            {
                var renderGo = new GameObject("[RenderingController]");
                renderGo.AddComponent<GameRenderingController>();
                renderGo.AddComponent<ProceduralSkyDarkener>();
            }
        }
    }
}
