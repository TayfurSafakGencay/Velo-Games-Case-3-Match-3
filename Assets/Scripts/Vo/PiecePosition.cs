using System;
using Enum;

namespace Vo
{
  [Serializable]
  public struct PiecePosition
  {
    public PieceType type;
    public int x;
    public int y;
  }
}