using System;
using System.Collections.Generic;
using System.Linq;
using Client.BoardMain;
using Client.Enum;
using Client.Vo;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Panel
{
  public class SkillPanel : MonoBehaviour
  {
    [SerializeField]
    private Board _board;

    [SerializeField]
    private GameObject _selectColorPanel;

    [Serializable]
    public struct Skill
    {
      public SkillKey SkillKey;
      
      public void SetSkillCount(int newValue)
      {
        SkillCount = newValue;
      }
      
      public int SkillCount;
      
      public Button SkillButton;
      
      public TextMeshProUGUI SkillCountText;
      
      public Image SkillCountBackground;
    }

    public List<Skill> SkillList;

    private readonly Dictionary<SkillKey, Skill> _skillDictionary = new();

    private void Awake()
    {
      for (int i = 0; i < SkillList.Count; i++)
      {
        Skill skill = SkillList[i];
        _skillDictionary.Add(skill.SkillKey, skill);
      }
      
      UpdateTexts();
      
      gameObject.SetActive(true);
      _selectColorPanel.gameObject.SetActive(false);
    }

    public void Init(List<SkillVo> skillVos)
    {
      for (int i = 0; i < skillVos.Count; i++)
      {
        SkillVo skillVo = skillVos[i];
        _skillDictionary[skillVo.SkillKey].SetSkillCount(skillVo.SkillCount);
      }

      UpdateTexts();
    }

    private const float _scaleButton = 1.15f;

    #region Paint

    public void OnSetActivePaintPanel()
    {
      if (_board.SkillKey == SkillKey.Empty)
      {
        _selectColorPanel.SetActive(true);
        _board.SetPieceClickable(false);
        
        _skillDictionary[SkillKey.Paint].SkillButton.transform.DOScale(_scaleButton, 0.5f);

        _board.SetSkillType(SkillKey.Paint);
         
         
      }
      else if (_board.SkillKey == SkillKey.Paint)
      {
        _selectColorPanel.SetActive(false);
        _board.SetPieceClickable(true);
        
        _skillDictionary[SkillKey.Paint].SkillButton.transform.DOScale(1, 0.5f);
        
        _board.SetSkillType(SkillKey.Empty);
      }
    }

    private ColorType _colorType;
    public void OnPaintPiece(int colorType)
    {
      _selectColorPanel.SetActive(false);
      _board.SetPieceClickable(true);
      
      _colorType = (ColorType)colorType;
    }

    public ColorType GetColorType()
    {
      return _colorType;
    }
    
    #endregion
    

    public void OnBreakPiece()
    {
      if (_board.SkillKey == SkillKey.Empty)
      {
        _board.SetSkillType(SkillKey.Break);

        _skillDictionary[SkillKey.Break].SkillButton.transform.DOScale(_scaleButton, 0.5f);
      }
      else if (_board.SkillKey == SkillKey.Break)
      {
        _board.SetSkillType(SkillKey.Empty);

        _skillDictionary[SkillKey.Break].SkillButton.transform.DOScale(1, 0.5f);
      }
    }

    public void DecreaseSkillCount(SkillKey skillKey)
    {
      Skill skill = _skillDictionary[skillKey];
      skill.SetSkillCount(skill.SkillCount - 1);
      _skillDictionary[skillKey] = skill;

      _skillDictionary[skillKey].SkillButton.transform.DOScale(1, 0.5f);

      UpdateText(skillKey);
    }
    

    private void UpdateTexts()
    {
      for (int i = 0; i < _skillDictionary.Count; i++)
      {
        UpdateText(_skillDictionary.ElementAt(i).Key);
      }
    }

    private void UpdateText(SkillKey skillKey)
    {
      Skill skill = _skillDictionary[skillKey];
      skill.SkillCountText.text = skill.SkillCount.ToString();
    }
  }
}