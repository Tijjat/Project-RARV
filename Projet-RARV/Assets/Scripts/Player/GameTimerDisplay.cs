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
                timerText.text = "";
                return;
            }

            GameManager manager = GameManager.Instance;

            if (!manager.GameLoaded.Value)
            {
                timerText.text = "Waiting...";
                return;
            }
            
            if (!manager.GameStarted.Value)
            {
                int seconds = Mathf.CeilToInt(
                    (float)manager.StartCountdown
                );

                timerText.text = seconds.ToString();

                return;
            }
            
            if (manager.GameStarted.Value && !manager.FirstArrived.Value)
                timerText.text = "";

            if (manager.FirstArrived.Value)
            {
                int seconds = Mathf.CeilToInt(
                    (float)manager.RemainingTime
                );
    
                timerText.text = seconds.ToString();
    
                if (manager.GameEnded.Value)
                    timerText.text = "0";
            }

            
        }
    }
}