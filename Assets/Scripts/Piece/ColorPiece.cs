using System.Collections.Generic;
using Enum;
using UnityEngine;
using Vo;

namespace Piece
{
  public class ColorPiece : MonoBehaviour
  {
    [SerializeField]
    private ColorType _color;
    
    [SerializeField]
    private List<ColorSprite> _colorSprites;
    
    private readonly Dictionary<ColorType, Sprite> _colorSpriteDictionary = new();

    private SpriteRenderer _spriteRenderer;

    public int ColorNumber => _colorSprites.Count;

    public ColorType Color
    {
      get => _color;
      set => _color = value;
    }

    private void Awake()
    {
      _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
      
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
      if (!_colorSpriteDictionary.TryGetValue(newColor, out Sprite value)) return;
    
      _spriteRenderer.sprite = value;
      Color = newColor;
    }

    public void SetSprite(Sprite sprite, ColorType colorType)
    {
      _spriteRenderer.sprite = sprite;
      Color = colorType;
    }
  }
}
