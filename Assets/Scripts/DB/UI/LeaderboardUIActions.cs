//Author: Tamer Erdoğan

using DB.FirestoreData;
using DB.Managers;
using TMPro;
using UnityEngine;

namespace DB.UI
{
    public class LeaderboardUIActions : MonoBehaviour
    {
        [SerializeField]
        private GameObject _leaderboardPanel;

        [SerializeField]
        private GameObject _leaderboardItemPrefab;

        [SerializeField]
        private Transform _leaderboardContentTransform;

        [SerializeField]
        private GameObject _leaderboardLoadingPanel;

        [SerializeField]
        private TMP_Text _leaderboardErrorText;

        void Start()
        {
            GetLeaderboarData();
        }

        public void LeaderboardToggle()
        {
            _leaderboardPanel.SetActive(!_leaderboardPanel.activeSelf);
        }

        public void GetLeaderboarData()
        {
            HideErrorMessage();
            DeleteAllItems();
            _leaderboardLoadingPanel.SetActive(true);

            DatabaseManager.Instance.GetLeaderboardData(
                (userDocs) =>
                {
                    foreach (var userDoc in userDocs)
                    {
                        UserFD user = userDoc.ConvertTo<UserFD>();
                        GameObject leaderboardItem = Instantiate(
                            _leaderboardItemPrefab,
                            _leaderboardContentTransform
                        );

                        Transform emailtextTransform = leaderboardItem.transform.Find("EmailText");
                        emailtextTransform.GetComponent<TMP_Text>().text =/* "Email: " + */user.email;

                        Transform scoreTextTransform = leaderboardItem.transform.Find("ScoreText");
                        scoreTextTransform.GetComponent<TMP_Text>().text =/*"Score: " +*/ user.score.ToString();
                    }

                    _leaderboardLoadingPanel.SetActive(false);
                },
                () =>
                {
                    ShowErrorMessage();
                    _leaderboardLoadingPanel.SetActive(false);
                }
            );
        }

        private void DeleteAllItems()
        {
            int childCount = _leaderboardContentTransform.childCount;
            for (int i = 0; i < childCount; i++)
                Destroy(_leaderboardContentTransform.GetChild(i).gameObject);
        }

        private void ShowErrorMessage()
        {
            _leaderboardErrorText.text = "An error occurred while retrieving leaderboard data.";
            _leaderboardErrorText.gameObject.SetActive(true);
        }

        private void HideErrorMessage()
        {
            _leaderboardErrorText.text = "";
            _leaderboardErrorText.gameObject.SetActive(false);
        }
    }
}
