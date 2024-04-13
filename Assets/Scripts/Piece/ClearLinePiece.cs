using System.Collections;
using DG.Tweening;
using Enum;
using UnityEngine;

namespace Piece
{
  public class ClearLinePiece : ClearablePiece
  {
    public bool IsRow;

    public override bool Clear()
    {
      if (IsStartedAnimation) return true;
      IsStartedAnimation = true;

      _piece.BoardRef.IncreaseDestroyingObjectCount();

      StartCoroutine(BeforeDestroyEffect(1));

      return true;
    }

    public override IEnumerator BeforeDestroyEffect(float time)
    {
      IsRow = _piece.PieceType switch
      {
        PieceType.RowClear => true,
        PieceType.ColumnClear => false,
        PieceType.SuperRocket => false,
        _ => throw new System.Exception("Error")
      };

      ExplosionAnimation();

      yield return new WaitForSeconds(time);
      
      _explosionEffect.Kill();

      if (_piece.PieceType == PieceType.SuperRocket)
      {
        _piece.BoardRef.RowRocket(_piece.Y);
        _piece.BoardRef.ColumnRocket(_piece.X);
      }
      else if (IsRow)
      {
        _piece.BoardRef.RowRocket(_piece.Y);
      }
      else if (!IsRow)
      {
        _piece.BoardRef.ColumnRocket(_piece.X);
      }
      
      _piece.BoardRef.Fillers();
      
      DestroyAnimation();
    }
  }
}