using System;
using Enum;
using UnityEngine;

namespace Vo
{
  [Serializable]
  public struct PiecePrefab
  {
    public PieceType Type;
    public GameObject Prefab;
  }
}