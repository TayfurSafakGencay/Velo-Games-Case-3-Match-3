using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using Enum;
using UnityEngine;

namespace Piece
{
  public class ClearLinePiece : ClearablePiece
  {
    public bool IsRow;

    [SerializeField]
    private GameObject _halfRocket;

    private bool _activated = false;

    private bool _isShouldBeDestroy;

    public void Activate()
    {
      _isShouldBeDestroy = true;

      if (_activated) return;
      _activated = true;
      
      IsRow = _piece.PieceType switch
      {
        PieceType.RowClear => true,
        PieceType.ColumnClear => false,
        PieceType.SuperRocket => false,
        _ => throw new System.Exception("Error")
      };
      
      _piece.BoardRef.IncreaseDestroyingObjectCount();
      
      BeforeDestroyEffect(500);
    }

    public override bool Clear()
    {
      if (_activated) return true;
      Activate();
      return true;
    }

    public async Task BeforeDestroyEffect(int time)
    {
      ExplosionAnimation();

      await Task.Delay(time);
      
      _explosionEffect.Kill();
      _spriteRenderer.enabled = false;

      if (_piece.PieceType == PieceType.SuperRocket)
      {
        Task task1 = _piece.BoardRef.RowRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
        Task task2 = _piece.BoardRef.ColumnRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);

        await Task.WhenAll(task1, task2);
      }
      else if (IsRow)
      {
        await _piece.BoardRef.RowRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }
      else if (!IsRow)
      {
        await _piece.BoardRef.ColumnRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }

      if (_isShouldBeDestroy)
        SpecialPieceDestroy();
    }
  }
}