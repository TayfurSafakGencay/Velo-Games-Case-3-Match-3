using System;
using System.Collections.Generic;
using Levels.Main;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Panel
{
    public class Hud : MonoBehaviour
    {
        public Level Level;

        public GameOverPanel GameOverPanel;

        [Header("Remaining Part")]
        [SerializeField]
        private TextMeshProUGUI _remainingText;

        [SerializeField]
        private TextMeshProUGUI _remainingTitleText;

        [Header("Target Score")]
        [SerializeField]
        private TextMeshProUGUI _targetScoreText;

        [SerializeField]
        private TextMeshProUGUI _targetTitleText;

        [Header("Score")]
        [SerializeField]
        private TextMeshProUGUI _currentScoreText;

        [SerializeField]
        private List<Image> _stars;

        private int _starIdx;

        private void Start()
        {
            SetStars();
        }

        public void SetScore(int score)
        {
            _currentScoreText.text = score.ToString();

            if (score >= Level.ScoreThirdStar)
            {
                _starIdx = 3;
            }
            else if (score >= Level.ScoreSecondStar)
            {
                _starIdx = 2;
            }
            else if (score >= Level.ScoreFirstStar)
            {
                _starIdx = 1;
            }
        
            SetStars();
        }

        private void SetStars()
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                _stars[i].color = _starIdx > i ? Color.yellow : Color.gray;
            }
        }

        public void SetTargetScore(int targetScore)
        {
            _targetScoreText.text = targetScore.ToString();
        }

        public void SetRemaining(int remaining)
        {
            _remainingText.text = remaining.ToString();
        }

        public void SetRemaining(string remaining)
        {
            _remainingText.text = remaining;
        }

        public void SetLevelType(Level.LevelType levelType)
        {
            switch (levelType)
            {
                case Level.LevelType.Moves:
                    SetTitles("Moves Remaining", "Target Score");
                    break;
                case Level.LevelType.Obstacle:
                    SetTitles("Moves Remaining", "Obstacles Remaining");
                    break;
                case Level.LevelType.Timer:
                    SetTitles("Time Remaining", "Target Score");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(levelType), levelType, null);
            }
        }

        private void SetTitles(string remainingTitleText, string targetTitleText)
        {
            _remainingTitleText.text = remainingTitleText;
            _targetTitleText.text = targetTitleText;
        }

        public void OnGameWin(int score)
        {
            GameOverPanel.OnGameWin(_starIdx, score, Level.LevelNumber);

            string levelName = SceneManager.GetActiveScene().name;

            if (_starIdx > PlayerPrefs.GetInt(levelName, 0))
            {
                PlayerPrefs.SetInt(levelName, _starIdx);
            }
        }

        public void OnGameLose(int score)
        {
            GameOverPanel.OnGameLose(_starIdx, score, Level.LevelNumber);
        }
    }
}
