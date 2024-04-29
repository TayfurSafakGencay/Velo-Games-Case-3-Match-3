using DB.FirestoreData;
using DB.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Panel.MainMenu
{
  public class MainMenu : MonoBehaviour
  {
    public TextMeshProUGUI PlayButtonText;

    private int _lastLevel;

    private void Awake()
    {
      DatabaseManager.Instance.GetCurrentUser(
        (UserFD user) =>
        {
          _lastLevel = user.level;
        },
        () =>
        {
          Debug.Log("An error occurred while retrieving user information.");
        });
        

      if (_lastLevel == 0) _lastLevel = 1;

      PlayButtonText.text = "Level " + _lastLevel;
    }

    public void OnPlay()
    {
      SceneManager.LoadScene("Level " + _lastLevel);
    }
  }
}