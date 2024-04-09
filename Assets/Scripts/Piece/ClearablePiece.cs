using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Piece
{
    public class ClearablePiece : MonoBehaviour
    {
        private bool _isBeingCleared;

        public bool IsBeingCleared => _isBeingCleared;

        protected GamePiece _piece;

        protected SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _piece = gameObject.GetComponent<GamePiece>();
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            _isBeingCleared = false;
        }

        public virtual bool Clear()
        {
            DestroyAnimation();

            return true;
        }

        private TweenerCore<Vector3, Vector3, VectorOptions> _scaleAnimation;

        private TweenerCore<Color, Color, ColorOptions> _fadeAnimation;
        protected void DestroyAnimation()
        {
            _isBeingCleared = true;

             _scaleAnimation = transform.DOScale(transform.localScale * 1.5f, 0.5f).SetEase(Ease.OutQuad);

            _fadeAnimation = _spriteRenderer.DOFade(0f, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                Destroy(gameObject);

                _piece.BoardRef.FinishDestroyingObjectCallers(0f);
            });
        }

        protected TweenerCore<Vector3, Vector3, VectorOptions> _explosionEffect;

        protected void ExplosionAnimation()
        {
            _explosionEffect = transform.DOScale(transform.localScale * 1.25f, 0.25f)
                .SetLoops(-1, LoopType.Yoyo) 
                .SetEase(Ease.InOutSine);
        }

        private void OnDestroy()
        {
            _scaleAnimation.Kill();
            _fadeAnimation.Kill();
            _explosionEffect.Kill();
        }

        public virtual IEnumerator BeforeDestroyEffect(float time)
        {
            yield break;
        }
    }
}
