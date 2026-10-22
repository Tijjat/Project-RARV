using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class EndGameScoreboard : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject endGamePanel;
        [SerializeField] private Transform scoreList;
        [SerializeField] private ScoreboardRow scoreRowPrefab;
        [SerializeField] private GameObject timerText;

        private bool _scoreboardDisplayed;

        private void Start()
        {
            endGamePanel.SetActive(false);
        }

        private void Update()
        {
            if (_scoreboardDisplayed)
                return;

            if (GameManager.Instance == null ||
                !GameManager.Instance.IsSpawned)
                return;

            if (!GameManager.Instance.GameEnded.Value)
                return;

            ShowScoreboard();
        }

        private void ShowScoreboard()
        {

            if (timerText != null)
                timerText.SetActive(false);

            _scoreboardDisplayed = true;

            // Collect player information.
            List<PlayerData> players = new List<PlayerData>();

            foreach (PlayerData data in FindObjectsByType<PlayerData>(FindObjectsSortMode.None))
            {
                if (data.IsSpawned)
                    players.Add(data);
            }

            players = players
                .OrderBy(p => p.PlayerTime.Value == 0)
                .ThenBy(p => p.PlayerTime.Value)
                .ToList();

            // Create one row per player.
            for (int i = 0; i < players.Count; i++)
            {
                ScoreboardRow row =
                    Instantiate(scoreRowPrefab, scoreList);
                int seconds = Mathf.CeilToInt(
                    (float)players[i].PlayerTime.Value
                );

                int minutes = seconds / 60;
                int remainingSeconds = seconds % 60;

                string timerText =
                    $"{minutes:00}:{remainingSeconds:00}";

                if (players[i].PlayerTime.Value == 0)
                {
                    row.Setup(
                        0,
                        players[i].PlayerPseudo.Value.ToString(),
                        "DNF"
                    );
                }
                else
                {
                    row.Setup(
                        i + 1,
                        players[i].PlayerPseudo.Value.ToString(),
                        timerText
                    );
                }
            }

            endGamePanel.SetActive(true);

            // Unlock the cursor to allow future menu interaction.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}