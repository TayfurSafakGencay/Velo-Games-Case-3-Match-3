using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Client.Panel.LevelSelectMenu
{
  public class LevelSelectPanelItem : MonoBehaviour
  {
    [SerializeField]
    private TextMeshProUGUI _levelText;

    [SerializeField]
    private List<Image> _stars;
    
    private int _level;

    private int _star;

    private bool _locked;
    public void Init(int level, int star, bool locked)
    {
      _level = level;
      _star = star;
      _locked = locked;

      _levelText.text = "Level " + _level;
      SetStars();
    }

    private void SetStars()
    {
      for (int i = 0; i < _stars.Count; i++)
      {
        _stars[i].color = _star > i ? Color.yellow : Color.gray;
      }
    }

    public void OpenLevel()
    {
      SceneManager.LoadScene("Level " + _level);
    }
  }
}