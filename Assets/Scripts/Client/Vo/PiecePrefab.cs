using System;
using Client.Enum;
using UnityEngine;

namespace Client.Vo
{
  [Serializable]
  public struct PiecePrefab
  {
    public PieceType Type;
    public GameObject Prefab;
  }
}