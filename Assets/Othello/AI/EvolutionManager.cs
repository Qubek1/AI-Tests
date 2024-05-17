using Othello;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public int numberOfPlayers;
    public int howManyPlayersWin;
    public float valueChangeRate;
    public float howOftenChanges;
    public int currentGeneration;
    public OthelloAI othelloAIA;
    public OthelloAI othelloAIB;
    public Queue<OthelloHeuristics> currentRoundHeuristics = new Queue<OthelloHeuristics>();
    public Queue<OthelloHeuristics> nextRoundHeuristics;
    public HeuristicsDisplay heuristicsDisplay;
    public OthelloGameModel gameModel;

    private void Start()
    {
        for (int i=0; i<howManyPlayersWin; i++)
        {
            currentRoundHeuristics.Enqueue(new OthelloHeuristics(GenerateNewRandomTable()));
        }
        StartNewGeneration();
    }

    private float[,,] GenerateNewRandomTable()
    {
        float[,,] fieldValueTable = new float[64, 64, 64];
        for (int i = 0; i < 64; i++)
        {
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    fieldValueTable[i, x, y] = Random.Range(-0.1f, 0.1f);
                }
            }
        }
        return fieldValueTable;
    }

    private void StartNewGeneration()
    {
        List<OthelloHeuristics> bestHeuristics = currentRoundHeuristics.ToList();
        for (int i = howManyPlayersWin; i < numberOfPlayers; i++)
        {
            OthelloHeuristics newHeuristic = CreateMerge(bestHeuristics);
            RandomlyChangeHeuristic(newHeuristic);
            currentRoundHeuristics.Enqueue(newHeuristic);
        }
        RunNewSimulation();
    }

    private void RandomlyChangeHeuristic(OthelloHeuristics heuristic)
    {
        for (int i = 0; i < 64; i++)
        {
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    if (Random.Range(0, 1) < howOftenChanges)
                    {
                        heuristic.fieldValueTable[i, x, y] += Random.Range(-valueChangeRate, valueChangeRate);
                    }
                }
            }
        }
    }

    private OthelloHeuristics CreateMerge(List<OthelloHeuristics> heuristicsList)
    {
        float[,,] newTable = new float[64,64,64];
        int randomHeuristicIndex = Random.Range(0, heuristicsList.Count);
        for (int i = 0; i < 64; i++)
        {
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    //float value = 0;
                    //foreach (var heuristic in heuristicsList)
                    //{
                    //    value += heuristic.fieldValueTable[i, x, y];
                    //}
                    //newTable[i, x, y] = value / heuristicsList.Count;
                    newTable[i, x, y] = heuristicsList[randomHeuristicIndex].fieldValueTable[i, x, y];
                }
            }
        }
        return new OthelloHeuristics(newTable);
    }

    private void RunNewSimulation()
    {
        gameModel = new OthelloGameModel();
        gameModel.gameUpdate += othelloAIA.OnGameUpdate;
        gameModel.gameUpdate += othelloAIB.OnGameUpdate;
        gameModel.gameUpdate += heuristicsDisplay.UIcontroller.OnGameUpdate;
        heuristicsDisplay.UIcontroller.gameModel = gameModel;
        gameModel.gameFinished += OnGameFinish;
        othelloAIA.Initialize((int)Time.time, 1, gameModel, currentRoundHeuristics.Dequeue());
        othelloAIB.Initialize((int)Time.time, 2, gameModel, currentRoundHeuristics.Dequeue());
        gameModel.StartGame();
    }

    private void OnGameFinish()
    {
        if (heuristicsDisplay != null)
        {
            heuristicsDisplay.heuristics = othelloAIA.heuristics;
            heuristicsDisplay.UpdateVisuals();
        }
        if (gameModel.score > 0)
        {
            nextRoundHeuristics.Enqueue(othelloAIA.heuristics);
        }
        else
        {
            nextRoundHeuristics.Enqueue(othelloAIB.heuristics);
        }

        if (currentRoundHeuristics.Count < 2)
        {
            if (currentRoundHeuristics.Count == 1)
            {
                nextRoundHeuristics.Enqueue(currentRoundHeuristics.Dequeue());
            }
            currentRoundHeuristics = nextRoundHeuristics;
            nextRoundHeuristics = new Queue<OthelloHeuristics>();
        }

        if (currentRoundHeuristics.Count + nextRoundHeuristics.Count > howManyPlayersWin)
        {
            RunNewSimulation();
        }
        else
        {
            StartNewGeneration();
        }
    }
}
