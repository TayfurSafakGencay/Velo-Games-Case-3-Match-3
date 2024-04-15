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
        Task task1 = _piece.BoardRef.RowRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
        Task task2 = _piece.BoardRef.ColumnRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }
      else if (IsRow)
      {
        Task task1 = _piece.BoardRef.RowRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }
      else if (!IsRow)
      {
        Task task1 = _piece.BoardRef.ColumnRocket(_halfRocket, _piece.X, _piece.Y, _piece.PieceType);
      }
    }
  }
}