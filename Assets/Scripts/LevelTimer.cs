using UnityEngine;

public class LevelTimer : Level
{
  [Header("Timer Level")]
  [SerializeField]
  private int _timeInSeconds;

  [SerializeField]
  private int _targetScore;

  private float _timer;

  private bool _timeFinished;

  private void Start()
  {
    Type = LevelType.Timer;
    
    Hud.SetLevelType(Type);
    
    Hud.SetTargetScore(_targetScore);
    Hud.SetRemaining($"{_timeInSeconds / 60}:{_timeInSeconds % 60}");

    _timer = _timeInSeconds;
  }

  private void Update()
  {
    if (_timeFinished)
      return;
    
    _timer -= Time.deltaTime;

    Hud.SetRemaining($"{(int)Mathf.Max(_timer / 60, 0)}:{(int)Mathf.Max(_timer % 60, 0)}");

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