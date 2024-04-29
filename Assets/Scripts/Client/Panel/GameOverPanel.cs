//Author: Şafak Gencay & Tamer Erdoğan

using System.Collections.Generic;
using DB.Helper;
using DB.Managers;
using Firebase.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Client.Panel
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        private TextMeshProUGUI _scoreText;

        [SerializeField]
        private List<Image> _stars;

        [SerializeField]
        private GameObject LoadingPanel;

        [SerializeField]
        private Button _doneButton;

        private int _score;

        private int _level;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void SetStars(int starCount)
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                _stars[i].color = starCount > i ? Color.yellow : Color.gray;
            }
        }

        public void OnGameWin(int star, int finalScore, int level)
        {
            gameObject.SetActive(true);

            _level = level;
            SetStars(star);

            _titleText.text = "You Win";
            _scoreText.text = finalScore.ToString();

            int newScore = SaveOnDeviceHelper.AddUserScore(finalScore);

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelUp);
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPostScore, "score", finalScore);
            
            DatabaseManager.Instance.GetCurrentUser();
            //TODO: Update işlemi async olduğu için, burada UI tarafında bir loading
            LoadingPanel.gameObject.SetActive(true);
            
            //hazırlanmalı ve loading state'i burada true yapılmalı
            DatabaseManager.Instance.UpdateUserFields(
                level,
                newScore,
                () => {
                    LoadingPanel.gameObject.SetActive(false);
                    gameObject.SetActive(true);
                    // TODO: Kullanıcı verisi başarıyla güncellendi. UI'ın loading'i kapatılmalı
                },
                () => {
                    LoadingPanel.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
                    // TODO: Kullanıcı verisi güncellenemedi. UI'ın loading'i kapatılmalı ve hata gibi birşey gösterilmeli
                }
            );
        }

        public void OnGameLose(int star, int finalScore, int level)
        {
            gameObject.SetActive(true);

            _doneButton.interactable = false;
            
            _level = level;
            SetStars(star);

            _titleText.text = "You Lose";
            _scoreText.text = finalScore.ToString();
        }

        public void OnReplayLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnHome()
        {
            SceneManager.LoadScene("Level Select");
        }

        public void OnNextLevel()
        {
            int newLevel = _level + 1;
            SceneManager.LoadScene("Level " + newLevel);
        }
    }
}
