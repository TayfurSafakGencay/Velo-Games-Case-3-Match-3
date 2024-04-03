using UnityEngine;

namespace Levels
{
    public class LevelMove : Main.Level
    {
        [Header("Move Level")]

        [SerializeField]
        private int _numberMoves;

        [SerializeField]
        private int _targetScore;

        private int _movesUsed = 0;

        protected override void Start()
        {
            base.Start();
            
            Type = LevelType.Moves;
        
            Hud.SetLevelType(Type);
            Hud.SetTargetScore(_targetScore);
            Hud.SetRemaining(_numberMoves);
        }

        public override void OnMove()
        {
            _movesUsed++;

            Hud.SetRemaining(_numberMoves - _movesUsed);

            if (_numberMoves - _movesUsed > 0) return;
            if (CurrentScore >= _targetScore)
            {
                GameWin();
            }
            else
            {
                GameLose();
            }
        }
    }
}