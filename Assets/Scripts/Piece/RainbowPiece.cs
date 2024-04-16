using System.Collections;
using DG.Tweening;
using Enum;
using UnityEngine;

namespace Piece
{
  public class RainbowPiece : ClearablePiece
  {
    private GamePiece _anotherPiece;

    private bool _activated;

    private bool _isShouldBeDestroy;

    private ColorType _anotherPieceColorType;

    public void SetPieces(GamePiece anotherPiece)
    {
      _anotherPiece = anotherPiece;
    }

    public void Activate()
    {
      if (_activated) return;
      _activated = true;
      
      _piece.BoardRef.IncreaseDestroyingObjectCount();
        
      StartCoroutine(BeforeDestroyEffect(0.25f));
    }
    public override bool Clear()
    {
      if (!_activated)
      {
        _isShouldBeDestroy = true;
        Activate();
        return false;
      }
      
      print("gg");
      SpecialPieceDestroy();

      return true;
    }
    
    public override IEnumerator BeforeDestroyEffect(float time)
    {      
      ExplosionAnimation();

      yield return new WaitForSeconds(time);
      
      _explosionEffect.Kill();

      if (_anotherPiece == null)
      {
        ColorType[] colorList = (ColorType[])System.Enum.GetValues(typeof(ColorType));
        System.Random random = new();
        ColorType randomColor = colorList[random.Next(0, 5)];

        _anotherPieceColorType = randomColor;
      }
      else
      {
        _anotherPieceColorType = ColorType.Any;
      }
      
      _piece.BoardRef.ClearRainbow(_piece, _anotherPiece, _anotherPieceColorType);
      
      if (_isShouldBeDestroy)
        SpecialPieceDestroy();
    }
  }
}