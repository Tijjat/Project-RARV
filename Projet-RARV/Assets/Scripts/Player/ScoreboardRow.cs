using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class ScoreboardRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text pseudoText;
        [SerializeField] private TMP_Text scoreText;

        public void Setup(int rank, string pseudo, int score)
        {
            rankText.text = rank.ToString();
            pseudoText.text = pseudo;
            scoreText.text = score.ToString();
        }
    }
}