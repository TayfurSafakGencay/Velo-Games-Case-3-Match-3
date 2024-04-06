using System;
using System.Collections.Generic;
using UnityEngine;

namespace Piece
{
  public class ColorPiece : MonoBehaviour
  {
    public enum ColorType
    {
      Red,
      Orange,
      Yellow,
      Green,
      Blue,
      Pink,
      Any,
      Count,
      Row,
      Column,
    }
  
    [Serializable]
    public struct ColorSprite
    {
      public ColorType Color;
      public Sprite Sprite;
    }

    [SerializeField]
    private List<ColorSprite> _colorSprites;

    private SpriteRenderer _sprite;

    private Dictionary<ColorType, Sprite> _colorSpriteDictionary = new();

    [SerializeField]
    private ColorType _color;

    public int ColorNumber => _colorSprites.Count;

    public ColorType Color
    {
      get => _color;
      set => _color = value;
    }

    private void Awake()
    {
      _sprite = gameObject.GetComponent<SpriteRenderer>();
      
      for (int i = 0; i < _colorSprites.Count; i++)
      {
        if (!_colorSpriteDictionary.ContainsKey(_colorSprites[i].Color))
        {
          _colorSpriteDictionary.Add(_colorSprites[i].Color, _colorSprites[i].Sprite);
        }
      }
    }

    public void SetColor(ColorType newColor)
    {
      if (!_colorSpriteDictionary.TryGetValue(newColor, out Sprite value))
      {
        
        print(newColor);
        return;
      }
    
      _sprite.sprite = value;
      Color = newColor;
    }
  }
}
