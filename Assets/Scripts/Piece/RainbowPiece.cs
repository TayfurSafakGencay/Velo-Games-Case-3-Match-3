using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Piece
{
  public class RainbowPiece : ClearablePiece
  {
    private GamePiece _rainbowPiece;

    private GamePiece _anotherPiece;

    public void SetPieces(GamePiece rainbowPiece, GamePiece anotherPiece)
    {
      _rainbowPiece = rainbowPiece;
      _anotherPiece = anotherPiece;
    }
    public override bool Clear()
    {
      StartCoroutine(BeforeDestroyEffect(0.5f));

      return true;
    }
    
    public override IEnumerator BeforeDestroyEffect(float time)
    {      
      ExplosionAnimation();

      yield return new WaitForSeconds(time);
      
      _explosionEffect.Kill();

      if (_rainbowPiece != null && _anotherPiece != null)
      {
        _piece.BoardRef.ClearRainbow(_rainbowPiece, _anotherPiece);
      }
      
      _piece.BoardRef.Fillers();
      
      DestroyAnimation();
    }
  }
}