using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
  public enum ColorType
  {
    Red,
    Orange,
    Yellow,
    Green,
    Black,
    Blue,
    White,
    Pink,
    Any,
    Count
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

  private ColorType _color;

  public int ColorNumber => _colorSprites.Count;

  public ColorType color
  {
    get => _color;
    set => SetColor(value);
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
    if (_colorSpriteDictionary.TryGetValue(newColor, out Sprite value))
    {
      _sprite.sprite = value;
    }
  }
}
