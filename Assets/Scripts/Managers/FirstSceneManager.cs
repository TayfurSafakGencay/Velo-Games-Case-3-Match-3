using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneManager : MonoBehaviour
{
    [SerializeField]
    private string _loadSceneName;

    void Start()
    {
        DontDestroyOnLoad(this);
        SceneManager.LoadScene(_loadSceneName);
    }
}
