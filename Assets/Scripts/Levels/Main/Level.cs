using System.Collections;
using BoardMain;
using Panel;
using Piece;
using UnityEngine;

namespace Levels.Main
{
    public class Level : MonoBehaviour
    {
        public enum LevelType
        {
            Timer,
            Obstacle,
            Moves,
        }

        [Header("References")]
        public Board Board;

        public Hud Hud;

        [Header("Score of Stars")]
        public int ScoreFirstStar;

        public int ScoreSecondStar;

        public int ScoreThirdStar;
    
        protected int CurrentScore { get; set; }

        protected LevelType Type {  get; set; }
    
        public bool DidWin { get; protected set; }

        private void Start()
        {
            Hud.SetScore(CurrentScore);
        }

        protected void GameWin()
        {
            Board.GameOver();

            DidWin = true;
        
            StartCoroutine(WaitForGridFill());
        }

        protected void GameLose()
        {
            Board.GameOver();

            DidWin = false;

            StartCoroutine(WaitForGridFill());
        }

        public virtual void OnMove()
        {
        }

        public virtual void OnPieceCleared(GamePiece piece)
        {
            CurrentScore += piece.Score;
        
            Hud.SetScore(CurrentScore);
        }

        protected virtual IEnumerator WaitForGridFill()
        {
            while (Board.IsFilling)
            {
                yield return 0;
            }

            if (DidWin )
            {
                Hud.OnGameWin(CurrentScore);
            }
            else
            {
                Hud.OnGameLose(CurrentScore);
            }
        }
    }
}
