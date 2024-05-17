using AI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Othello
{
    public class OthelloGameModel
    {
        public int[,] boardState;
        public int boardLength;
        public int boardHeight;
        public int startingPlayer;
        public int currentPlayersTurn;
        public int score;
        public int turnCounter;
        public int emptyFields;

        private List<Vector2Int> directions;

        public delegate void GameEvents();
        public event GameEvents gameUpdate;
        public event GameEvents gameFinished;

        public OthelloGameModel()
        {
            startingPlayer = 1;
            boardLength = 8;
            boardHeight = 8;
            emptyFields = 60;
            SetupBoard();
            InitializeDirections();
        }

        public OthelloGameModel(OthelloSaveData saveData)
        {
            LoadSave(saveData);
            InitializeDirections();
        }

        public void StartGame()
        {
            turnCounter = 0;
            score = 0;
            currentPlayersTurn = startingPlayer;
            SetupBoard();
            gameUpdate();
        }

        public bool ValidateMove(OthelloMove move)
        {
            if (currentPlayersTurn != move.team || boardState[move.position.x, move.position.y] != 0)
            {
                return false;
            }
            foreach (Vector2Int direction in directions)
            {
                int distance = GoInDirection(move.position, direction, move.team);
                if (distance > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void MakeMove(OthelloMove move)
        {
            boardState[move.position.x, move.position.y] = move.team;
            score -= move.team * 2 - 3;
            foreach (Vector2Int direction in directions)
            {
                int distance = GoInDirection(move.position, direction, move.team);
                if (distance > 0)
                {
                    Vector2Int currentPosition = move.position;
                    for (int i = 0; i < distance; i++)
                    {
                        score -= move.team * 2 - 3;
                        if (boardState[currentPosition.x, currentPosition.y] != 0)
                        {
                            score -= move.team * 2 - 3;
                        }
                        currentPosition += direction;
                        boardState[currentPosition.x, currentPosition.y] = move.team;
                    }
                }
            }
            currentPlayersTurn = currentPlayersTurn % 2 + 1;
            turnCounter++;
            emptyFields--;
            if (emptyFields == 0 || GetPossibleMoves().Count == 0)
            {
                gameFinished();
                return;
            }
            if (gameUpdate != null)
            {
                gameUpdate();
            }
        }

        public List<AIMove> GetPossibleMoves()
        {
            // TODO: allocate space for list once
            List<AIMove> possibleMoves = new List<AIMove>();
            for (int x = 0; x < boardLength; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    OthelloMove move = new OthelloMove();
                    move.position = new Vector2Int(x, y);
                    move.team = currentPlayersTurn;
                    if (ValidateMove(move))
                    {
                        possibleMoves.Add(new OthelloAIMove(move.team, move));
                    }
                }
            }
            return possibleMoves;
        }

        private void SetupBoard()
        {
            boardState = new int[boardLength, boardHeight];
            boardState[3, 3] = 1;
            boardState[4, 4] = 1;
            boardState[3, 4] = 2;
            boardState[4, 3] = 2;
        }

        private int GoInDirection(Vector2Int position, Vector2Int direction, int team)
        {
            Vector2Int currentPosition = position + direction;
            if (!ValidatePosition(currentPosition))
            {
                return 0;
            }
            int distance = 0;
            while (boardState[currentPosition.x, currentPosition.y] != team && boardState[currentPosition.x, currentPosition.y] != 0)
            {
                currentPosition += direction;
                if (!ValidatePosition(currentPosition))
                {
                    return 0;
                }
                distance++;
            }
            if (boardState[currentPosition.x, currentPosition.y] == team)
            {
                return distance;
            }
            return 0;
        }

        private bool ValidatePosition(Vector2Int position)
        {
            if (position.x < 0 | position.y < 0)
            {
                return false;
            }
            if (position.x >= boardLength || position.y >= boardHeight)
            {
                return false;
            }
            return true;
        }

        private void InitializeDirections()
        {
            directions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1)
        };
        }

        public void LoadSave(OthelloSaveData saveData)
        {
            boardLength = saveData.boardState.GetLength(0);
            boardHeight = saveData.boardState.GetLength(1);
            boardState = new int[8, 8];
            emptyFields = 0;
            for (int x = 0; x < boardLength; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    boardState[x,y] = saveData.boardState[x, y];
                    if (boardState[x, y] == 0)
                    {
                        emptyFields++;
                    }
                }
            }
            currentPlayersTurn = saveData.currentPlayerTurn;
            score = saveData.score;
        }

        public OthelloSaveData MakeSaveData()
        {
            OthelloSaveData saveData = new OthelloSaveData();
            saveData.boardState = new int[boardLength, boardHeight];
            for (int x = 0; x < boardLength; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    saveData.boardState[x, y] = boardState[x, y];
                }
            }
            saveData.currentPlayerTurn = currentPlayersTurn;
            return saveData;
        }
    }

    public class OthelloMove
    {
        public int team;
        public Vector2Int position;
    }
}