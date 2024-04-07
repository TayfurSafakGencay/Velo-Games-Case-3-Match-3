using System.Collections.Generic;
using UnityEngine;

namespace Piece
{
  public class ObstaclePiece : ClearablePiece
  {
    [SerializeField]
    private List<Sprite> _sprites;

    private int _health;

    private void Start()
    {
      _health = 4;

      _spriteRenderer.sprite = _sprites[_health - 1];
    }

    public override bool Clear()
    {
      _health--;

      if (_health == 0)
      {
        return base.Clear();
      }

      _spriteRenderer.sprite = _sprites[_health - 1];

      return false;
    }
  }
}