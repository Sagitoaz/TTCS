using System.Collections.Generic;
using TTCS.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.TeamFormation
{
    /// <summary>
    /// Day 4 team setup UI controller.
    /// Handles lineup selection (max 3), validation, and navigation back to main menu.
    /// </summary>
    public class TeamFormationUIController : MonoBehaviour
    {
        [SerializeField] private Button _validateButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _statusText;

        private readonly List<string> _selectedLineup = new List<string>(3);
        private ITeamService _teamService;

        private void Start()
        {
            _teamService = FlowController.Instance.GetComponent<ITeamService>();
            if (_teamService == null)
            {
                Debug.LogWarning("[TeamFormation] TeamService not found, using local validation fallback");
            }

            if (_validateButton != null)
            {
                _validateButton.onClick.AddListener(OnValidateClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            UpdateStatus("Select up to 3 characters");
        }

        /// <summary>
        /// Hook this from character item buttons in the TeamFormation scene.
        /// </summary>
        public void OnCharacterClicked(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            if (_selectedLineup.Contains(characterId))
            {
                _selectedLineup.Remove(characterId);
                UpdateStatus($"Removed: {characterId} ({_selectedLineup.Count}/3)");
                return;
            }

            if (_selectedLineup.Count >= 3)
            {
                UpdateStatus("Lineup full (max 3)");
                return;
            }

            _selectedLineup.Add(characterId);
            UpdateStatus($"Added: {characterId} ({_selectedLineup.Count}/3)");
        }

        public void ClearSelection()
        {
            _selectedLineup.Clear();
            UpdateStatus("Selection cleared");
        }

        private void OnValidateClicked()
        {
            if (_selectedLineup.Count == 0)
            {
                UpdateStatus("Please select at least 1 character");
                return;
            }

            ValidationResult result = _teamService != null
                ? _teamService.ValidateLineup(_selectedLineup)
                : LocalValidate(_selectedLineup);

            if (!result.Valid)
            {
                UpdateStatus(string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Invalid lineup"
                    : result.ErrorMessage);
                return;
            }

            _teamService?.SaveLineup(_selectedLineup);
            UpdateStatus("Lineup validated and saved");
        }

        private void OnBackClicked()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private static ValidationResult LocalValidate(IReadOnlyList<string> lineup)
        {
            if (lineup.Count > 3)
            {
                return new ValidationResult
                {
                    Valid = false,
                    ErrorMessage = "Lineup must have at most 3 characters"
                };
            }

            return new ValidationResult { Valid = true };
        }

        private void UpdateStatus(string message)
        {
            Debug.Log($"[TeamFormation] {message}");
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }
    }
}
