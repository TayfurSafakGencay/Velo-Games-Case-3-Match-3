using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Piece
{
  public class BombPiece : ClearablePiece
  {
    public override bool Clear()
    {
      StartCoroutine(BeforeDestroyEffect(1));
      
      return true;
    }

    public override IEnumerator BeforeDestroyEffect(float time)
    {      
      ExplosionAnimation();

      yield return new WaitForSeconds(time);
      
      _explosionEffect.Kill();

      _piece.BoardRef.ClearBomb(_piece);
      
      _piece.BoardRef.Fillers();
      
      DestroyAnimation();
    }
  }
}