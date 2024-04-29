using System;
using Client.Enum;
using UnityEngine;

namespace Client.Vo
{
  [Serializable]
  public struct ColorSprite
  {
    public ColorType Color;
    
    public Sprite Sprite;
  }
}