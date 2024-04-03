using UnityEngine;

namespace Levels
{
  public class LevelTimer : Main.Level
  {
    [Header("Timer Level")]
    [SerializeField]
    private int _timeInSeconds;

    [SerializeField]
    private int _targetScore;

    private float _timer;

    private bool _timeFinished;

    protected override void Start()
    {
      base.Start();
      
      Type = LevelType.Timer;
    
      Hud.SetLevelType(Type);
    
      Hud.SetTargetScore(_targetScore);
      
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

      if (!(_timer <= 0)) return;
      if (CurrentScore >= _targetScore)
      {
        GameWin();
      }
      else
      {
        GameLose();
      }

      _timeFinished = true;
    }
  }
}