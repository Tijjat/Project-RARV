using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class ScoreboardRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text pseudoText;
        [SerializeField] private TMP_Text timerText;

        public void Setup(int rank, string pseudo, string timer)
        {
            rankText.text = rank == 0 ? "" : rank.ToString();
            pseudoText.text = pseudo;
            timerText.text = timer;
        }
    }
}