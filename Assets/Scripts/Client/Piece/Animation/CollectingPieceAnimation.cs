using System.Collections.Generic;
using Client.Panel;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Client.Piece.Animation
{
  public class CollectingPieceAnimation : MonoBehaviour
  {
    [SerializeField]
    private RectTransform _target;

    [SerializeField]
    private GameObject _animatedObject;

    [SerializeField]
    private Hud _hud;

    [Space(20)]
    [SerializeField]
    private int _maxObjects;

    private readonly  Queue<GameObject> _objectQueue = new();

    [Space]
    [Header("Animation Settings")]
    [SerializeField]
    private Ease _easeType;

    private Vector3 _targetPosition;

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
        newObject = Instantiate(_animatedObject, transform.position, Quaternion.identity, transform);
        newObject.SetActive(false);
        _objectQueue.Enqueue(newObject);
      }
    }

    private const float _minDuration = 0.8f;

    private const float _maxDuration = 1.25f;

    private void AnimateObject(Vector3 objectPosition, int score)
    {
      GameObject animatedObject = _objectQueue.Dequeue();

      animatedObject.SetActive(true);
      animatedObject.transform.position = objectPosition;

      float duration = Random.Range(_minDuration, _maxDuration);
      animatedObject.transform.DOMove(_targetPosition, duration).SetEase(_easeType).OnComplete(() =>
      {
        animatedObject.SetActive(false);
        _objectQueue.Enqueue(animatedObject);

        _hud.SetScore(score);
      });
    }

    public void AddObjects(Vector3 objectPosition, int score)
    {
      AnimateObject(objectPosition, score);
    }
  }
}