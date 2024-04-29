using Client.Enum;
using Client.Levels.Main;
using Client.Piece;
using UnityEngine;

namespace Client.Levels
{
  public class LevelBonus : Level
  {
    [Header("Bonus Level")]
    [SerializeField]
    private int _timeInSeconds;

    private float _timer;

    private bool _timeFinished;

    protected override void Start()
    {
      base.Start();
      
      Type = LevelType.Bonus;
      Hud.SetLevelType(Type);
    
      _timer = _timeInSeconds;
    }

    private void Update()
    {
      if (_timeFinished)
      {
        Hud.SetRemaining("0:00");
        return;
      }
    
      _timer -= Time.deltaTime;

      Hud.SetRemaining($"{Mathf.FloorToInt(_timer / 60)}:{Mathf.FloorToInt(_timer % 60):00}");

      if (_timer > 0) return;
      
      GameWin();
      _timeFinished = true;
    }

    private const float _starChance = 20f;
    public override void OnPieceCleared(GamePiece piece)
    {
      if (piece.PieceType != PieceType.Normal) return;

      if (Random.Range(0, 101) > _starChance) return;

      CurrentScore++;
      StartCoroutine(Board.StartDestroyAnimation(piece, CurrentScore));
    }
  }
}