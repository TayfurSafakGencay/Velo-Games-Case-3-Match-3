using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Levels.Main;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

namespace Panel
{
    public class Hud : MonoBehaviour
    {
        public Level Level;

        public GameOverPanel GameOverPanel;

        [Header("Moves/Times")]
        [SerializeField]
        private TextMeshProUGUI _remainingText;

        [SerializeField]
        private TextMeshProUGUI _remainingTitleText;

        [Header("Target Score")]
        [SerializeField]
        private TextMeshProUGUI _targetScoreText;

        [Header("Score")]
        [SerializeField]
        private TextMeshProUGUI _currentScoreText;

        [SerializeField]
        private List<Image> _stars;

        private int _starIdx;

        private float _newScore;

        private void Start()
        {
            SetStars();

            _currentScoreText.text = 0.ToString();
        }

        public void SetScore(int score)
        {
            StartCoroutine(CountUpToTarget(score));
        }

        private const float _targetScale = 1.25f;

        private const float _duration = 0.5f;

        private const Ease _ease = Ease.InFlash;

        private bool _isContinuousCount;

        private float _speed = 1f;
        
        private IEnumerator CountUpToTarget(int score)
        {
            _currentScoreText.transform.DOScale(_targetScale, _duration).SetEase(_ease).OnComplete(() =>
            {
                _currentScoreText.transform.DOScale(Vector3.one, _duration).SetEase(_ease);
            });

            if (!_isContinuousCount)
                _speed = 1f;
            
            while (_newScore < score)
            {
                _isContinuousCount = true;
                
                _speed += 1f;
                _newScore += Time.deltaTime * _speed;
                
                _newScore = Mathf.Clamp(_newScore, 0, score);
                _currentScoreText.text = _newScore.ToString("f0");
                
                yield return null;
            }

            _isContinuousCount = false;
            
            _newScore = score;
            SetStars();
        }

        private void SetStars()
        {
            if (_newScore >= Level.ScoreThirdStar)
            {
                _starIdx = 3;
            }
            else if (_newScore >= Level.ScoreSecondStar)
            {
                _starIdx = 2;
            }
            else if (_newScore >= Level.ScoreFirstStar)
            {
                _starIdx = 1;
            }
            
            for (int i = 0; i < _stars.Count; i++)
            {
                _stars[i].color = _starIdx > i ? Color.white : Color.gray;
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
                    SetTitles("Moves");
                    break;
                case Level.LevelType.Obstacle:
                    SetTitles("Moves");
                    break;
                case Level.LevelType.Timer:
                    SetTitles("Time");
                    break;
                case Level.LevelType.Bonus:
                    SetTitles("Time");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(levelType), levelType, null);
            }
        }

        private void SetTitles(string remainingTitleText)
        {
            _remainingTitleText.text = remainingTitleText;
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
