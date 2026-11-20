// Central lifecycle state machine: handles New Game, Continue, Pause, MainMenu, and play time tracking.
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LayeredHell
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public bool IsPlaying => State == GameState.Playing;
        public float PlayTimeSeconds { get; set; }

        private Coroutine _transitionRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            GameSettings.EnsureLoaded();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (State == GameState.Playing)
            {
                PlayTimeSeconds += Time.deltaTime;
            }

            if ((State == GameState.Playing || State == GameState.Paused) && InputRouter.MenuPressed)
            {
                if (GameMenuUI.Instance != null)
                {
                    GameMenuUI.Instance.Toggle();
                }
                else
                {
                    Pause(State == GameState.Playing);
                }
            }
        }

        public void SetState(GameState newState)
        {
            if (State == newState)
            {
                UpdateTimeAndCursor(newState);
                return;
            }

            State = newState;
            UpdateTimeAndCursor(newState);
            GameEvents.RaiseStateChanged(newState);
        }

        private static void UpdateTimeAndCursor(GameState state)
        {
            Time.timeScale = (state == GameState.Paused) ? 0f : 1f;

            bool lockCursor = (state == GameState.Playing) && !Application.isMobilePlatform && !MobileControls.IsTouchActive;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
        }

        public void StartNewGame()
        {
            if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);
            _transitionRoutine = StartCoroutine(StartNewGameRoutine());
        }

        private IEnumerator StartNewGameRoutine()
        {
            Time.timeScale = 1f;
            PlayTimeSeconds = 0f;

            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.DeleteSave(0);
            }

            if (Application.CanStreamedLevelBeLoaded("GameWorld"))
            {
                AsyncOperation op = SceneManager.LoadSceneAsync("GameWorld", LoadSceneMode.Single);
                if (op != null)
                {
                    while (!op.isDone) yield return null;
                }
            }

            if (Inventory.Instance != null) Inventory.Instance.ResetForNewGame();
            if (QuestManager.Instance != null) QuestManager.Instance.ResetForNewGame();
            if (JournalSystem.Instance != null) JournalSystem.Instance.ResetForNewGame();
            if (RadioSystem.Instance != null) RadioSystem.Instance.ResetForNewGame();
            if (BaseManager.Instance != null)
            {
                BaseManager.Instance.SendMessage("ResetForNewGame", SendMessageOptions.DontRequireReceiver);
            }

            if (FloorStreamer.Instance != null)
            {
                FloorStreamer.Instance.BeginGame(GameConstants.BaseFloor);
                float timeout = 6f;
                while (FloorStreamer.Instance.IsBusy && timeout > 0f)
                {
                    timeout -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            Vector3 spawnPos = FloorStreamer.Instance != null
                ? FloorStreamer.Instance.GetPlayerSpawn(GameConstants.BaseFloor)
                : new Vector3(0f, GameConstants.FloorY(GameConstants.BaseFloor) + 0.1f, 0f);

            PlayerController player = PlayerFactory.Spawn(spawnPos, Quaternion.identity);
            if (player != null && player.Stats != null)
            {
                player.Stats.Respawn();
            }

            SetState(GameState.Playing);
            GameEvents.RaiseFloorChanged(GameConstants.BaseFloor, GameConstants.BaseFloor);

            if (AudioManager.Instance != null)
            {
                AudioManager.PlayMusic("mus_safehouse", true, 2f);
            }

            _transitionRoutine = null;
        }

        public void ContinueGame()
        {
            if (SaveSystem.Instance == null || !SaveSystem.Instance.HasSave(0))
            {
                StartNewGame();
                return;
            }

            if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);
            _transitionRoutine = StartCoroutine(ContinueGameRoutine());
        }

        private IEnumerator ContinueGameRoutine()
        {
            Time.timeScale = 1f;

            if (Application.CanStreamedLevelBeLoaded("GameWorld"))
            {
                AsyncOperation op = SceneManager.LoadSceneAsync("GameWorld", LoadSceneMode.Single);
                if (op != null)
                {
                    while (!op.isDone) yield return null;
                }
            }

            Vector3 initialPos = new Vector3(0f, GameConstants.FloorY(GameConstants.BaseFloor) + 0.1f, 0f);
            PlayerFactory.Spawn(initialPos, Quaternion.identity);

            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Load(0);
            }

            if (FloorStreamer.Instance != null)
            {
                float timeout = 6f;
                while (FloorStreamer.Instance.IsBusy && timeout > 0f)
                {
                    timeout -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            SetState(GameState.Playing);
            int curFloor = FloorStreamer.Instance != null ? FloorStreamer.Instance.CurrentFloor : GameConstants.BaseFloor;
            GameEvents.RaiseFloorChanged(curFloor, curFloor);

            _transitionRoutine = null;
        }

        public void Pause(bool paused)
        {
            if (paused && State == GameState.Playing)
            {
                SetState(GameState.Paused);
                if (SaveSystem.Instance != null) SaveSystem.Instance.Save(0);
            }
            else if (!paused && State == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void ReturnToMainMenu()
        {
            if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);
            _transitionRoutine = StartCoroutine(ReturnToMainMenuRoutine());
        }

        private IEnumerator ReturnToMainMenuRoutine()
        {
            if (State == GameState.Playing || State == GameState.Paused)
            {
                if (SaveSystem.Instance != null) SaveSystem.Instance.Save(0);
            }

            Time.timeScale = 1f;

            if (PlayerController.Instance != null)
            {
                Destroy(PlayerController.Instance.gameObject);
            }

            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
                if (op != null)
                {
                    while (!op.isDone) yield return null;
                }
            }

            SetState(GameState.MainMenu);
            _transitionRoutine = null;
        }
    }
}
