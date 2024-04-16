using System.Collections;
using DG.Tweening;
using Enum;
using UnityEngine;

namespace Piece
{
  public class BombPiece : ClearablePiece
  {
    private bool _activated;

    private bool _isShouldBeDestroy;

    private float _destroyEffectTime = 0.5f;
    public void Activate()
    {
      if (_activated)
      {
        return;
      }
      _activated = true;
      
      _piece.BoardRef.IncreaseDestroyingObjectCount();

      if (_piece.PieceType == PieceType.SuperBomb)
      {
        _destroyEffectTime = 1f;
      }
      
      StartCoroutine(BeforeDestroyEffect(_destroyEffectTime));
    }

    public override bool Clear()
    {
      if (!_activated)
      {
        _isShouldBeDestroy = true;
        Activate();
        return false;
      }
      
      SpecialPieceDestroy();
      
      return true;
    }

    public override IEnumerator BeforeDestroyEffect(float time)
    {      
      ExplosionAnimation();

      yield return new WaitForSeconds(time);
      
      _explosionEffect.Kill();
      
      int radius = _piece.PieceType == PieceType.SuperBomb ? 3 : 1;
      _piece.BoardRef.ClearBomb(_piece, radius);

      if (_isShouldBeDestroy)
      {
        SpecialPieceDestroy();
      }
    }
  }
}