using DG.Tweening;
using UnityEngine;

namespace Piece.Animation
{
    public class ClearLinePieceAnimation : MonoBehaviour
    {
        private bool _isRow;

        private Sequence _sequence;

        private MovablePiece _movablePiece;

        private void Awake()
        {
            _isRow = gameObject.GetComponent<ClearLinePiece>().IsRow;
            _movablePiece = gameObject.GetComponent<MovablePiece>();
        }

        private void Start()
        {
            StartAnimation();

            _movablePiece.OnMove += StartAnimation;
        }

        private void StartAnimation()
        {
            if (_isRow)
            {
                RowAnimation();
            }
            else
            {
                ColumnAnimation();
            }
        }

        private void RowAnimation()
        {
            const float limitValue = 0.05f;
            float xPos = transform.localPosition.x;
        
            _sequence?.Kill();

            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOLocalMoveX(xPos + limitValue, 0.5f).SetEase(Ease.OutQuad));
            _sequence.Append(transform.DOLocalMoveX(xPos - limitValue, 0.5f).SetEase(Ease.OutQuad));
            _sequence.SetLoops(-1, LoopType.Restart);
        }

        private void ColumnAnimation()
        {
            const float limitValue = 0.05f;
            float yPos = transform.localPosition.y;
        
            _sequence?.Kill();
        
            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOLocalMoveY(yPos + limitValue, 0.5f).SetEase(Ease.OutQuad));
            _sequence.Append(transform.DOLocalMoveY(yPos - limitValue, 0.5f).SetEase(Ease.OutQuad));
            _sequence.SetLoops(-1, LoopType.Restart);
        }

        private void OnDestroy()
        {
            _sequence.Kill();

            _movablePiece.OnMove -= StartAnimation;
        }
    }
}
