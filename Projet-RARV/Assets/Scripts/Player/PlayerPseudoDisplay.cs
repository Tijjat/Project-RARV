using TMPro;
using Unity.Collections;
using UnityEngine;

namespace Gameplay
{
    public class PlayerPseudoDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text pseudoText;

        private PlayerData _playerData;

        private void Start()
        {
            _playerData = GetComponentInParent<PlayerData>();

            if (_playerData == null)
                return;

            _playerData.PlayerPseudo.OnValueChanged += OnPseudoChanged;

            UpdatePseudo(_playerData.PlayerPseudo.Value);
        }

        private void OnDestroy()
        {
            if (_playerData == null)
                return;

            _playerData.PlayerPseudo.OnValueChanged -= OnPseudoChanged;
        }

        private void OnPseudoChanged(
            FixedString64Bytes previous,
            FixedString64Bytes current)
        {
            UpdatePseudo(current);
        }

        private void UpdatePseudo(FixedString64Bytes pseudo)
        {
            if (pseudoText != null)
                pseudoText.text = pseudo.ToString();
        }
    }
}