// Runtime settings persisted via PlayerPrefs. Used by InputRouter, AudioManager, PlayerController, and UI.
using UnityEngine;

namespace LayeredHell
{
    public static class GameSettings
    {
        private const string KeyLookSens = "lh45_look_sens";
        private const string KeyTouchSens = "lh45_touch_sens";
        private const string KeyMasterVol = "lh45_master_vol";
        private const string KeySfxVol = "lh45_sfx_vol";
        private const string KeyMusicVol = "lh45_music_vol";
        private const string KeyAmbienceVol = "lh45_amb_vol";
        private const string KeyInvertY = "lh45_invert_y";
        private const string KeyFov = "lh45_fov";
        private const string KeyHeadBob = "lh45_headbob";

        private static bool _loaded;

        public static float LookSensitivity { get; set; } = 1.0f;
        public static float TouchSensitivity { get; set; } = 0.22f;
        public static float MasterVolume { get; set; } = 1.0f;
        public static float SfxVolume { get; set; } = 1.0f;
        public static float MusicVolume { get; set; } = 0.7f;
        public static float AmbienceVolume { get; set; } = 0.8f;
        public static bool InvertY { get; set; } = false;
        public static float FieldOfView { get; set; } = 75f;
        public static bool HeadBob { get; set; } = true;

        public static void EnsureLoaded()
        {
            if (!_loaded) Load();
        }

        public static void Load()
        {
            _loaded = true;
            LookSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(KeyLookSens, 1.0f), 0.1f, 5.0f);
            TouchSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(KeyTouchSens, 0.22f), 0.05f, 2.0f);
            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMasterVol, 1.0f));
            SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeySfxVol, 1.0f));
            MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMusicVol, 0.7f));
            AmbienceVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyAmbienceVol, 0.8f));
            InvertY = PlayerPrefs.GetInt(KeyInvertY, 0) == 1;
            FieldOfView = Mathf.Clamp(PlayerPrefs.GetFloat(KeyFov, 75f), 55f, 105f);
            HeadBob = PlayerPrefs.GetInt(KeyHeadBob, 1) == 1;
            AudioListener.volume = MasterVolume;
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(KeyLookSens, LookSensitivity);
            PlayerPrefs.SetFloat(KeyTouchSens, TouchSensitivity);
            PlayerPrefs.SetFloat(KeyMasterVol, MasterVolume);
            PlayerPrefs.SetFloat(KeySfxVol, SfxVolume);
            PlayerPrefs.SetFloat(KeyMusicVol, MusicVolume);
            PlayerPrefs.SetFloat(KeyAmbienceVol, AmbienceVolume);
            PlayerPrefs.SetInt(KeyInvertY, InvertY ? 1 : 0);
            PlayerPrefs.SetFloat(KeyFov, FieldOfView);
            PlayerPrefs.SetInt(KeyHeadBob, HeadBob ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = MasterVolume;
        }

        public static void ResetToDefaults()
        {
            LookSensitivity = 1.0f;
            TouchSensitivity = 0.22f;
            MasterVolume = 1.0f;
            SfxVolume = 1.0f;
            MusicVolume = 0.7f;
            AmbienceVolume = 0.8f;
            InvertY = false;
            FieldOfView = 75f;
            HeadBob = true;
            Save();
        }
    }
}
