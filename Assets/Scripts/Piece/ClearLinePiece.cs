using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Enum;
using UnityEngine;

namespace Piece
{
  public class ClearLinePiece : ClearablePiece
  {
    public bool IsRow;

    public GameObject HalfRocket;
    
    public override bool Clear()
    {
      StartCoroutine(BeforeDestroyEffect(0.25f));

      return true;
    }

    public override IEnumerator BeforeDestroyEffect(float time)
    {
      IsRow = _piece.PieceType switch
      {
        PieceType.RowClear => true,
        PieceType.ColumnClear => false,
        _ => throw new System.Exception("Error")
      };

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
        _piece.BoardRef.RowRocket(HalfRocket, _piece.X, _piece.Y);
      }
      else if (!IsRow)
      {
        _piece.BoardRef.ColumnRocket(HalfRocket, _piece.X, _piece.Y);
      }
      
      DirectDestroy();
    }
  }
}