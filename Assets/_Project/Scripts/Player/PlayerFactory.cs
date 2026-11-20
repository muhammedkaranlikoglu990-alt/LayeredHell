// Factory that builds or repositions the runtime first-person Player hierarchy with camera, arms, flashlight, and components.
using UnityEngine;

namespace LayeredHell
{
    public static class PlayerFactory
    {
        public static PlayerController Spawn(Vector3 pos, Quaternion rot)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.Teleport(pos, rot);
                return PlayerController.Instance;
            }

            GameObject root = new GameObject("Player");
            int playerLayer = LayerMask.NameToLayer(LayerNames.Player);
            if (playerLayer >= 0) root.layer = playerLayer;
            Object.DontDestroyOnLoad(root);

            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 48f;
            cc.skinWidth = 0.04f;

            PlayerStats stats = root.AddComponent<PlayerStats>();
            FootstepPlayer footsteps = root.AddComponent<FootstepPlayer>();
            PlayerController controller = root.AddComponent<PlayerController>();
            WeaponController weapons = root.AddComponent<WeaponController>();

            // Build FPS Camera
            GameObject camGo = new GameObject("PlayerCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(root.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            camGo.transform.localRotation = Quaternion.identity;

            Camera cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = GameSettings.FieldOfView;

            // Ensure only one AudioListener is active
            AudioListener existingListener = Object.FindFirstObjectByType<AudioListener>();
            if (existingListener != null && existingListener.gameObject != camGo)
            {
                existingListener.enabled = false;
            }
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<Interactor>();

            // Build Flashlight
            GameObject lightGo = new GameObject("Flashlight");
            lightGo.transform.SetParent(camGo.transform, false);
            lightGo.transform.localPosition = new Vector3(0.14f, -0.08f, 0.15f);
            lightGo.transform.localRotation = Quaternion.identity;

            Light spot = lightGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 24f;
            spot.spotAngle = 58f;
            spot.innerSpotAngle = 32f;
            spot.intensity = 2.4f;
            spot.color = new Color(1.0f, 0.95f, 0.85f);
            spot.shadows = LightShadows.Soft;
            spot.enabled = true;

            // Attach FPS arms if present in ModelLibrary
            Animator armsAnimator = null;
            if (ModelLibrary.Instance != null)
            {
                GameObject armsPrefab = ModelLibrary.Instance.Get("chr_fps_arms_01");
                if (armsPrefab != null)
                {
                    GameObject armsInstance = Object.Instantiate(armsPrefab, camGo.transform, false);
                    armsInstance.name = "FPSArms";
                    armsInstance.transform.localPosition = new Vector3(0f, -0.24f, 0.18f);
                    armsInstance.transform.localRotation = Quaternion.identity;
                    armsAnimator = armsInstance.GetComponentInChildren<Animator>();
                }
            }

            controller.CameraTransform = camGo.transform;
            controller.Stats = stats;
            controller.Weapons = weapons;
            controller.Flashlight = spot;
            controller.ArmsAnimator = armsAnimator;

            controller.Teleport(pos, rot);
            return controller;
        }
    }
}
