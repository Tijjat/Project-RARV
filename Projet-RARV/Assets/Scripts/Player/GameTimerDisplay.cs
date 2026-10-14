using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class GameTimerDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        private void Update()
        {
            if (GameManager.Instance == null ||
                !GameManager.Instance.IsSpawned)
            {
                timerText.text = "--:--";
                return;
            }

            GameManager manager = GameManager.Instance;

            if (!manager.GameStarted.Value)
            {
                timerText.text = "Waiting...";
                return;
            }

            int seconds = Mathf.CeilToInt(
                (float)manager.RemainingTime
            );

            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            timerText.text =
                $"{minutes:00}:{remainingSeconds:00}";

            if (manager.GameEnded.Value)
                timerText.text = "00:00";
        }
    }
}