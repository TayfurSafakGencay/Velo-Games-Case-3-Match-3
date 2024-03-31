using UnityEngine;
using UnityEngine.SceneManagement;

namespace Panel
{
    public class LevelSelectPanel : MonoBehaviour
    {
        public void OpenLevel(string levelName)
        {
            SceneManager.LoadScene(levelName);
        }
    }
}