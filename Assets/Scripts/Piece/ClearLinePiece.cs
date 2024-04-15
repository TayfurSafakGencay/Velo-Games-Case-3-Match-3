using System.Collections;
using DG.Tweening;
using Enum;
using UnityEngine;

namespace Piece
{
  public class ClearLinePiece : ClearablePiece
  {
    public bool IsRow;

    public GameObject HalfRocket;

    public override void Activate()
    {
      IsRow = _piece.PieceType switch
      {
        PieceType.RowClear => true,
        PieceType.ColumnClear => false,
        PieceType.SuperRocket => false,
        _ => throw new System.Exception("Error")
      };
      
      _piece.BoardRef.IncreaseDestroyingObjectCount();

      
      StartCoroutine(BeforeDestroyEffect(0.25f));
    }
    public override bool Clear()
    {
      SpecialPieceDestroy();

      return true;
    }

    public override IEnumerator BeforeDestroyEffect(float time)
    {
      ExplosionAnimation();

      yield return new WaitForSeconds(time);

      _explosionEffect.Kill();

      if (_piece.PieceType == PieceType.SuperRocket)
      {
        // _piece.BoardRef.RowRocket(HalfRocket, _piece.X, _piece.Y);
        // _piece.BoardRef.ColumnRocket(_piece.X);
      }
      else if (IsRow)
      {
        _piece.BoardRef.RowRocket(HalfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }
      else if (!IsRow)
      {
        _piece.BoardRef.ColumnRocket(HalfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }
    }
  }
}