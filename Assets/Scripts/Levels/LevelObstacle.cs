using System.Collections;
using BoardMain;
using Piece;
using UnityEngine;

namespace Levels
{
    public class LevelObstacle : Main.Level
    {
        [Header("Obstacle Level")]

        [SerializeField]
        private int _numberMoves;

        private int _movesUsed;

        public Board.PieceType[] ObstacleTypes;

        private int _numberObstacleLeft;

        protected override void Start()
        {
            base.Start();
            
            Type = LevelType.Obstacle;
        
            for (int i = 0; i < ObstacleTypes.Length; i++)
            {
                _numberObstacleLeft += Board.GetTypeOfPieces(ObstacleTypes[i]).Count;
            }
        
            Hud.SetLevelType(Type);
            Hud.SetTargetScore(_numberObstacleLeft);
            Hud.SetRemaining(_numberMoves);
        }

        public override void OnMove()
        {
            base.OnMove();

            _movesUsed++;
            Hud.SetRemaining(_numberMoves - _movesUsed);
        
            if (_numberMoves - _movesUsed <= 0 && _numberObstacleLeft > 0)
            {
                GameLose(); 
            }
        }

        public override void OnPieceCleared(GamePiece piece)
        {
            base.OnPieceCleared(piece);

            for (int i = 0; i < ObstacleTypes.Length; i++)
            {
                if (ObstacleTypes[i] != piece.Type) continue;
            
                _numberObstacleLeft--;
                Hud.SetTargetScore(_numberObstacleLeft);

                if (_numberObstacleLeft != 0) continue;
                
                CurrentScore += 1000 * (_numberMoves - _movesUsed);
                Hud.SetScore(CurrentScore);

                StartCoroutine(WaitGameEnd());
            }
        }

        private IEnumerator WaitGameEnd()
        {
            yield return new WaitForSeconds(0.5f);
            
            GameWin();
        }
    }
}