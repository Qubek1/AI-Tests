using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Othello;
using AI;
using Unity.VisualScripting;
using System.Threading;

public class ANNOthelloTraining : MonoBehaviour
{
    public string fileName;
    public float learningSpeed = 0.1f;
    public int batchSize = 5;
    public int batchGroupSize = 200;
    public ANNLayerCalculation layerCalculation;

    OthelloGameModel gameModel;
    public ANN ann;
    float currentCostAverage = 0;
    int currentSimulationIndex = 0;
    private System.Random rng = new System.Random();
    
    private List<float[]> TrainingInputs;
    private List<float[]> TrainingOutputs;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < 100; i++)
            {
                ann.CalculateWithDerivatives(new float[128]);
            }
            Debug.Log("Done");
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 0; i < 100; i++)
            {
                ann.NewCalculateWithDerivatives(new float[128]);
            }
            Debug.Log("Done");
        }
    }

    public void GenerateRandomNetwork()
    {
        ann = new ANN(128, 500, 500, 500, 500, 64);
        ann.Save(fileName);
        ann.learningSpeed = learningSpeed;
        ann.layerCalculations = layerCalculation;
    }

    public void LoadExistingNewtork()
    {
        ann = new ANN(fileName);
    }

    public void StartTraining()
    {
        if (TrainingInputs != null && TrainingInputs.Count > 0)
        {
            while (TrainingInputs.Count > batchSize)
            {
                float[][] batchInputs = new float[batchSize][];
                float[][] batchOutputs = new float[batchSize][];

                //chosing random training data
                for (int i = 0; i < batchSize; i++)
                {
                    int randomIndex = rng.Next(0, TrainingInputs.Count);
                    batchInputs[i] = TrainingInputs[randomIndex];
                    batchOutputs[i] = TrainingOutputs[randomIndex];
                    TrainingInputs.RemoveAt(randomIndex);
                    TrainingOutputs.RemoveAt(randomIndex);
                }

                float currentCost = ann.Train(batchInputs, batchOutputs) / batchSize;
                if (currentCostAverage < 0.1f)
                {
                    currentCostAverage = currentCost;
                }
                currentCostAverage = currentCostAverage * 0.5f + currentCost * 0.5f;
                Debug.Log(currentCostAverage);
            }
        }
        Thread thread = new Thread(new ThreadStart(StartSimulations));
        thread.Start();
    }

    public void StartSimulations()
    {
        currentCostAverage = 0;
        TrainingInputs = new List<float[]>();
        TrainingOutputs = new List<float[]>();
        while (true)
        {
            ann.learningSpeed = learningSpeed;
            //batchInputs = new List<float[]>();
            //batchOutputs = new List<float[]>();
            gameModel = new OthelloGameModel();
            gameModel.gameUpdate += OnGameUpdate;
            gameModel.gameFinished += OnGameFinish;
            gameModel.StartGame();
            return;
        }
    }

    public void OnGameUpdate()
    {
        float[] inputLayer = new float[128];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                
                inputLayer[x + y * 8] = gameModel.boardState[x, y] == gameModel.currentPlayersTurn ? 1 : -1;
                inputLayer[64 + x + y * 8] = 
                    (gameModel.boardState[x, y] != gameModel.currentPlayersTurn && gameModel.boardState[x, y] != 0) ? 1 : -1;
            }
        }
        float[] expectedOutput = new float[64];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                expectedOutput[x + y * 8] = -1;
            }
        }

        List<AIMove> possibleMoves = gameModel.GetPossibleMoves();
        foreach (AIMove move in possibleMoves)
        {
            OthelloAIMove othelloMove = (OthelloAIMove)move;
            expectedOutput[othelloMove.othelloMove.position.x + othelloMove.othelloMove.position.y * 8] = 1;
        }

        TrainingInputs.Add(inputLayer);
        TrainingOutputs.Add(expectedOutput);

        if (TrainingInputs.Count >= batchGroupSize)
        {
            float[][] batchInputs = new float[batchSize][];
            float[][] batchOutputs = new float[batchSize][];

            //chosing random training data
            for (int i = 0; i < batchSize; i++)
            {
                int randomIndex = rng.Next(0, TrainingInputs.Count);
                batchInputs[i] = TrainingInputs[randomIndex];
                batchOutputs[i] = TrainingOutputs[randomIndex];
                TrainingInputs.RemoveAt(randomIndex);
                TrainingOutputs.RemoveAt(randomIndex);
            }

            float currentCost = ann.Train(batchInputs, batchOutputs) / batchSize;
            if (currentCostAverage < 0.1f)
            {
                currentCostAverage = currentCost;
            }
            currentCostAverage = currentCostAverage * 0.5f + currentCost * 0.5f;
            Debug.Log(currentCostAverage);
        }

        //currentCost += ann.Train(inputLayer, expectedOutput);
        gameModel.MakeMove(((OthelloAIMove)possibleMoves[rng.Next(0, possibleMoves.Count)]).othelloMove);
    }

    public void OnGameFinish()
    {
        Debug.Log("Simulation " + currentSimulationIndex.ToString() + " finished");
        //Debug.Log((currentCost / (currentSimulationIndex + 1)).ToString());
        SaveANN();
        currentSimulationIndex++;
    }

    private void SaveANN()
    {
        ann.Save(fileName);
    }
}
