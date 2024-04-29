//Author: Tamer Erdoğan

using Firebase.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DB.Managers
{
    public class FirstSceneManager : MonoBehaviour
    {
        [SerializeField]
        private string _loadSceneName;

        void Start()
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
            DontDestroyOnLoad(this);
            SceneManager.LoadScene(_loadSceneName);
        }
    }
}
