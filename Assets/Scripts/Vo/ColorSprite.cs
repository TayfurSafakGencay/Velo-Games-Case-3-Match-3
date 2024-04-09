using System;
using Enum;
using UnityEngine;

namespace Vo
{
  [Serializable]
  public struct ColorSprite
  {
    public ColorType Color;
    
    public Sprite Sprite;
  }
}