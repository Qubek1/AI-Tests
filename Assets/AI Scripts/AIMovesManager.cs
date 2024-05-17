using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public interface IGameModelForAI
    {
        public void ResetGameState();
        public List<AIMove> GetPossibleMoves();
        public void MakeMove(AIMove move);
        public float Rollout();
        public void CalculationFinished();
    }

    public abstract class AIMove
    {
        public int team;
        public abstract bool SameMove(AIMove move);
        public AIMove(int team)
        {
            this.team = team;
        }
    }
}