// Interactive hinged door supporting keys, lockpicks, smooth local-Y rotation, audio, and save state.
using System;
using UnityEngine;

namespace LayeredHell
{
    public sealed class Door : MonoBehaviour, IInteractable, ISaveable
    {
        public string keyId = string.Empty;
        public bool locked;
        public bool lockpickable = true;
        public float openAngle = 100f;
        public string doorId = string.Empty;

        private bool _isOpen;
        private float _baseYaw;
        private float _currentOffsetYaw;
        private bool _initializedYaw;

        [Serializable]
        private struct SaveData
        {
            public bool locked;
            public bool isOpen;
        }

        public string SaveKey
        {
            get
            {
                if (!string.IsNullOrEmpty(doorId))
                {
                    return "door_" + doorId;
                }
                Vector3 p = transform.position;
                int hx = Mathf.RoundToInt(p.x * 10f);
                int hy = Mathf.RoundToInt(p.y * 10f);
                int hz = Mathf.RoundToInt(p.z * 10f);
                return "door_" + hx + "_" + hy + "_" + hz;
            }
        }

        public static Door Setup(GameObject go, string doorId, string keyId = "", bool locked = false)
        {
            if (go == null) return null;

            if (go.GetComponentInChildren<Collider>() == null)
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 1.05f, 0f);
                box.size = new Vector3(1.0f, 2.1f, 0.14f);
            }

            Door door = go.GetComponent<Door>();
            if (door == null) door = go.AddComponent<Door>();
            door.doorId = doorId ?? string.Empty;
            door.keyId = keyId ?? string.Empty;
            door.locked = locked;
            return door;
        }

        private void Awake()
        {
            CaptureBaseYaw();
        }

        private void OnEnable()
        {
            CaptureBaseYaw();
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Unregister(this);
            }
        }

        private void CaptureBaseYaw()
        {
            if (_initializedYaw) return;
            _initializedYaw = true;
            _baseYaw = transform.localEulerAngles.y;
        }

        private void Update()
        {
            float targetOffset = _isOpen ? openAngle : 0f;
            if (Mathf.Abs(_currentOffsetYaw - targetOffset) > 0.05f)
            {
                _currentOffsetYaw = Mathf.Lerp(_currentOffsetYaw, targetOffset, Time.deltaTime * 8f);
                Vector3 e = transform.localEulerAngles;
                e.y = _baseYaw + _currentOffsetYaw;
                transform.localEulerAngles = e;
            }
        }

        public bool CanInteract()
        {
            return true;
        }

        public string GetPrompt()
        {
            if (locked)
            {
                if (!string.IsNullOrEmpty(keyId) && Inventory.Instance != null && Inventory.Instance.Has(keyId))
                {
                    ItemDef def = ItemDatabase.Get(keyId);
                    string keyName = def != null ? def.name : keyId;
                    return "Kilidi Aç (" + keyName + ")";
                }
                if (lockpickable && Inventory.Instance != null && Inventory.Instance.Has("lockpick"))
                {
                    return "Maymuncukla Aç";
                }
                if (!string.IsNullOrEmpty(keyId))
                {
                    ItemDef def = ItemDatabase.Get(keyId);
                    string keyName = def != null ? def.name : keyId;
                    return "Kilitli (" + keyName + " Gerekli)";
                }
                return "Kilitli Kapı";
            }

            return _isOpen ? "Kapıyı Kapat" : "Kapıyı Aç";
        }

        public void Interact(PlayerController player)
        {
            if (locked)
            {
                if (!string.IsNullOrEmpty(keyId) && Inventory.Instance != null && Inventory.Instance.Has(keyId))
                {
                    locked = false;
                    _isOpen = true;
                    AudioManager.Play("door_open", transform.position, 0.9f);
                    GameEvents.RaiseToast("Kapı anahtarla açıldı.");
                    PlayArmsAnim(player);
                    return;
                }

                if (lockpickable && Inventory.Instance != null && Inventory.Instance.Has("lockpick"))
                {
                    Inventory.Instance.Remove("lockpick", 1);
                    locked = false;
                    _isOpen = true;
                    AudioManager.Play("lockpick", transform.position, 0.9f);
                    AudioManager.Play("door_open", transform.position, 0.85f);
                    GameEvents.RaiseToast("Kilit maymuncukla açıldı.");
                    PlayArmsAnim(player);
                    return;
                }

                AudioManager.Play("door_locked", transform.position, 0.9f);
                if (!string.IsNullOrEmpty(keyId))
                {
                    ItemDef def = ItemDatabase.Get(keyId);
                    string keyName = def != null ? def.name : keyId;
                    GameEvents.RaiseToast("Kapı kilitli: " + keyName + " gerekiyor.");
                }
                else
                {
                    GameEvents.RaiseToast("Kapı kilitli.");
                }
                return;
            }

            _isOpen = !_isOpen;
            AudioManager.Play(_isOpen ? "door_open" : "door_close", transform.position, 0.85f);
            NoiseSystem.Emit(transform.position, 5f, gameObject);
            PlayArmsAnim(player);
        }

        private static void PlayArmsAnim(PlayerController player)
        {
            if (player != null && player.ArmsAnimator != null)
            {
                player.ArmsAnimator.CrossFadeInFixedTime("FPS_DoorOpen", 0.1f);
            }
        }

        public string CaptureState()
        {
            SaveData data = new SaveData
            {
                locked = locked,
                isOpen = _isOpen
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            CaptureBaseYaw();
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            locked = data.locked;
            _isOpen = data.isOpen;
            _currentOffsetYaw = _isOpen ? openAngle : 0f;
            Vector3 e = transform.localEulerAngles;
            e.y = _baseYaw + _currentOffsetYaw;
            transform.localEulerAngles = e;
        }
    }
}
