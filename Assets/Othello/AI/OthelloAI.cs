using AI;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Othello
{
    public class OthelloAI : MonoBehaviour, IGameModelForAI
    {
        public int team;
        public OthelloGameModel originalGameModel;
        public int rngSeed;
        public bool calculating = false;

        public TreeSearchParameters treeSearchParameters;

        private OthelloGameModel gameModelCopy;
        private OthelloSaveData currentGameState;
        private TreeSearch treeSearch;
        private System.Random rand;
        private Task searchTreeCalculations;
        private Stopwatch stopwatch;
        public OthelloHeuristics heuristics;

        public void Initialize(int rngSeed, int team, OthelloGameModel gameModel, OthelloHeuristics heuristics)
        {
            this.rngSeed = rngSeed;
            this.team = team;
            this.heuristics = heuristics;
            originalGameModel = gameModel;
            rand = new System.Random(rngSeed);
        }

        private void Update()
        {
            if (originalGameModel != null && !calculating && originalGameModel.currentPlayersTurn == team)
            {
                PlayBestMoveFromTree();
            }
        }

        public void OnGameUpdate()
        {
            if (calculating || originalGameModel.currentPlayersTurn != team)
            {
                return;
            }
            currentGameState = originalGameModel.MakeSaveData();
            gameModelCopy = new OthelloGameModel(currentGameState);
            treeSearch = new TreeSearch(this, treeSearchParameters);

            stopwatch = new Stopwatch();
            stopwatch.Start();
            calculating = true;
            searchTreeCalculations = Task.Factory.StartNew(() => { treeSearch.StartCalculations(); });
        }

        public void ResetGameState()
        {
            gameModelCopy.LoadSave(currentGameState);
        }

        public void MakeMove(AIMove move)
        {
            OthelloMove othelloMove = ((OthelloAIMove)move).othelloMove;
            gameModelCopy.MakeMove(othelloMove);
        }

        public float Rollout()
        {
            float beforeRolloutHeuristics = heuristics.Calculate(team, gameModelCopy);

            List<AIMove> possibleMoves = GetPossibleMoves();
            while (possibleMoves.Count > 0)
            {
                int randomIndex = rand.Next(0, possibleMoves.Count);
                gameModelCopy.MakeMove(((OthelloAIMove)possibleMoves.ToArray()[randomIndex]).othelloMove);
                possibleMoves = GetPossibleMoves();
            }
            if (gameModelCopy.score == 0)
            {
                return beforeRolloutHeuristics;
            }
            if (gameModelCopy.score > 0 && team == 1)
            {
                return gameModelCopy.score * gameModelCopy.score / 200f + 1 + beforeRolloutHeuristics;
            }
            else
            {
                return -(gameModelCopy.score * gameModelCopy.score / 200f + 1) + beforeRolloutHeuristics;
            }
        }

        public void PlayBestMoveFromTree()
        {
            float bestScore = float.MinValue;
            Node winningNode = treeSearch.root;
            foreach(Node child in treeSearch.root.children)
            {
                if (child.value > bestScore)
                {
                    winningNode = child;
                    bestScore = child.value;
                }
            }
            originalGameModel.MakeMove(((OthelloAIMove)winningNode.move).othelloMove);
        }

        public void CalculationFinished()
        {
            stopwatch.Stop();
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
            calculating = false;
        }

        public List<AIMove> GetPossibleMoves()
        {
            return gameModelCopy.GetPossibleMoves();
        }
    }

    public class OthelloAIMove : AIMove
    {
        public OthelloMove othelloMove;

        public OthelloAIMove(int team, OthelloMove othelloMove) : base(team)
        {
            this.othelloMove = othelloMove;
        }

        public override bool SameMove(AIMove move)
        {
            OthelloAIMove otherMove = (OthelloAIMove)move;
            return otherMove.othelloMove.position == othelloMove.position && otherMove.othelloMove.position == othelloMove.position;
        }
    }
}