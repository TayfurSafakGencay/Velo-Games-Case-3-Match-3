using System;
using BoardMain;
using Enum;
using TMPro;
using UnityEngine;

namespace Panel
{
  public class SkillPanel : MonoBehaviour
  {
    [SerializeField]
    private Board _board;

    [SerializeField]
    private GameObject _selectColorPanel;

    [Header("Skill Count")]
    [SerializeField]
    private TextMeshProUGUI _paintPieceText;

    private int _paintPieceCount;
    
    [SerializeField]
    private TextMeshProUGUI _breakPieceText;

    private int _breakPieceCount;

    private void Awake()
    {
      _selectColorPanel.gameObject.SetActive(false);
    }

    public void Init(int breakPieceCount, int paintPieceCount)
    {
      _paintPieceCount = paintPieceCount;
      _breakPieceCount = breakPieceCount;

      UpdateTexts();
    }

    #region Paint

    public void OnSetActivePaintPanel()
    {
      if (_selectColorPanel.activeInHierarchy)
      {
        _selectColorPanel.SetActive(false);
        _board.SetPieceClickable(true);
      }
      else
      {
        _selectColorPanel.SetActive(true);
        _board.SetPieceClickable(false);
      }
    }

    private ColorType _colorType;
    public void OnPaintPiece(int colorType)
    {
      OnSetActivePaintPanel();
      
      _board.SetSkillType(SkillType.Paint);
      _colorType = (ColorType)colorType;
    }

    public ColorType GetColorType()
    {
      return _colorType;
    }
    
    #endregion
    

    public void OnBreakPiece()
    {
      _board.SetSkillType(SkillType.Break);
    }

    public void DecreaseSkillCount(SkillType skillType)
    {
      switch (skillType)
      {
        case SkillType.Break:
          _breakPieceCount--;
          break;
        case SkillType.Paint:
          _paintPieceCount--;
          break;
        case SkillType.Empty:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      
      UpdateTexts();
    }
    

    private void UpdateTexts()
    {
      _paintPieceText.text = _paintPieceCount.ToString();
      _breakPieceText.text = _breakPieceCount.ToString();
    }
  }
}