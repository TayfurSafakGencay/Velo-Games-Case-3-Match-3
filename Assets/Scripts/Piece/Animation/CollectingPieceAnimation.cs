using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Piece.Animation
{
    public class CollectingPieceAnimation : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _target;

        [Space(20)]
        
        [SerializeField]
        private int _maxObjects;

        private Dictionary<ColorPiece.ColorType, Queue<GameObject>> _objectQueues = new();

        [Space]
        [Header("Animation Settings")]
        
        [SerializeField]
        private Ease _easeType;
        
        private Vector3 _targetPosition;

        [Space]
        [Header("Animated Object List")]
        
        [SerializeField]
        private List<AnimatedObject> _animatedObjects;
        
        [Serializable]
        public struct AnimatedObject
        {
            public ColorPiece.ColorType ColorType;
            public GameObject AnimatedGameObject;
        }

        private void Awake()
        {
            _targetPosition = _target.position;
            
            PrepareObjects();
        }

        private void PrepareObjects()
        {
            GameObject newObject;
            
            for (int i = 0; i < _maxObjects; i++)
            {
                for (int j = 0; j < _animatedObjects.Count; j++)
                {
                    AnimatedObject animObject = _animatedObjects[j];
                    newObject = Instantiate(animObject.AnimatedGameObject, transform.position, quaternion.identity, transform);
                    newObject.SetActive(false);

                    if (!_objectQueues.ContainsKey(animObject.ColorType))
                    {
                        _objectQueues.Add(animObject.ColorType, new Queue<GameObject>());
                    }
                    
                    _objectQueues[animObject.ColorType].Enqueue(newObject);
                }
            }
        }

        private const float _minDuration = 0.8f;

        private const float _maxDuration = 1.25f;

        private void AnimateObject(Vector3 objectPosition, GamePiece piece)
        {
            // if (piece.ColorComponent == null)
            // {
            //     return;
            // }
            // ColorPiece.ColorType color = piece.ColorComponent.Color;
            //
            // if(!_objectQueues.ContainsKey(color)) return;
            //
            // if (_objectQueues[color].Count <= 0) return;
            //
            // GameObject animatedObject = _objectQueues[color].Dequeue();
            //
            //     
            // animatedObject.SetActive(true);
            // animatedObject.transform.position = objectPosition;
            //
            // float duration = Random.Range(_minDuration, _maxDuration);
            // animatedObject.transform.DOMove(_targetPosition, duration).SetEase(_easeType).OnComplete(() =>
            // {
            //     animatedObject.SetActive(false);
            //     _objectQueues[color].Enqueue(animatedObject);
            //     
            //     piece.BoardRef.Level.OnPieceCleared(piece);
            // });
        }

        public void AddObjects(Vector3 objectPosition, GamePiece piece)
        {
            AnimateObject(objectPosition, piece);
        }
    }
}
