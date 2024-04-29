using DB.FirestoreData;
using DB.Helper;
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
      UserFD UserFd = SaveOnDeviceHelper.GetUser();

      _lastLevel = UserFd.level;

      PlayButtonText.text = "Level " + _lastLevel;
    }

    public void OnPlay()
    {
      SceneManager.LoadScene("Level " + _lastLevel);
    }
  }
}