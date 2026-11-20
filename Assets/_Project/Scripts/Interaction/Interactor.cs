// First-person camera raycaster for IInteractable objects within 3m reach.
using UnityEngine;

namespace LayeredHell
{
    public sealed class Interactor : MonoBehaviour
    {
        public const float ReachDistance = 3.0f;

        private IInteractable _currentTarget;
        private string _lastPrompt = string.Empty;

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                ClearPrompt();
                return;
            }

            if (PlayerController.Instance != null && PlayerController.Instance.InputLocked)
            {
                ClearPrompt();
                return;
            }

            _currentTarget = null;
            string prompt = string.Empty;

            Ray ray = new Ray(transform.position, transform.forward);
            int mask = ~LayerNames.PlayerMask;

            if (Physics.Raycast(ray, out RaycastHit hit, ReachDistance, mask, QueryTriggerInteraction.Collide))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    _currentTarget = interactable;
                    prompt = interactable.GetPrompt() ?? string.Empty;
                }
            }

            SetPromptIfChanged(prompt);

            if (_currentTarget != null && InputRouter.InteractPressed)
            {
                _currentTarget.Interact(PlayerController.Instance);
                // Re-query prompt immediately after interaction
                string postPrompt = _currentTarget.CanInteract() ? (_currentTarget.GetPrompt() ?? string.Empty) : string.Empty;
                SetPromptIfChanged(postPrompt);
            }
        }

        private void ClearPrompt()
        {
            _currentTarget = null;
            SetPromptIfChanged(string.Empty);
        }

        private void SetPromptIfChanged(string prompt)
        {
            if (_lastPrompt == prompt) return;
            _lastPrompt = prompt;
            if (HUD.Instance != null)
            {
                HUD.Instance.SetPrompt(prompt);
            }
        }
    }
}
