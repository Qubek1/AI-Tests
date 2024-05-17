using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class TreeSearch
    {
        IGameModelForAI gameModel;
        TreeSearchParameters parameters;
        private float[] lnArray;
        private int maxNodePathLenght = 1000;
        public Node root;
        public int AIteam;

        public TreeSearch (IGameModelForAI gameModel, TreeSearchParameters parameters)
        {
            this.gameModel = gameModel;
            this.parameters = parameters;
        }

        public void StartCalculations()
        {
            GenerateLnArray(parameters.IterationsCount);
            root = new Node(AIteam);
            Node currentNode;

            for (int i = 0; i < parameters.IterationsCount; i++)
            {
                currentNode = root;
                for (int j = 0; j < maxNodePathLenght; j++)
                {
                    currentNode.visitCount++;
                    List<Node> possibleNodes = GetPossibleNodes(currentNode);
                    if (possibleNodes.Count == 0)
                    {
                        break;
                    }
                    currentNode = ChooseBestNode(currentNode, possibleNodes);
                    gameModel.MakeMove(currentNode.move);
                    if (currentNode.visitCount == 0)
                    {
                        currentNode.visitCount++;
                        break;
                    }
                }
                float score = gameModel.Rollout();
                BackPropagation(currentNode, score);

                gameModel.ResetGameState();
            }
            gameModel.CalculationFinished();
        }

        private List<Node> GetPossibleNodes(Node currentNode)
        {
            List<AIMove> possibleMoves = gameModel.GetPossibleMoves();
            List<Node> possibleNodes = new List<Node>();
            Node[] children = currentNode.children.ToArray();

            foreach (AIMove move in possibleMoves)
            {
                bool alreadyGenerated = false;
                for (int i = 0; i < children.Length; i++)
                {
                    if (move.SameMove(children[i].move))
                    {
                        alreadyGenerated = true;
                        children[i].move = move;
                        possibleNodes.Add(children[i]);
                        break;
                    }
                }
                if (!alreadyGenerated)
                {
                    Node newNode = new Node(move, currentNode);
                    possibleNodes.Add(newNode);
                    currentNode.children.Add(newNode);
                }
            }
            return possibleNodes;
        }

        private Node ChooseBestNode(Node currentNode, List<Node> possibleNodes)
        {
            Node bestNode = new Node();
            float bestScore = float.MinValue;

            foreach (Node node in possibleNodes)
            {
                if (node.visitCount == 0)
                {
                    return node;
                }
                float nodeScore = node.value / (float)node.visitCount +
                    parameters.explorationValue * Mathf.Sqrt(lnArray[currentNode.visitCount] / (float)node.visitCount);
                if (nodeScore > bestScore)
                {
                    bestScore = nodeScore;
                    bestNode = node;
                }
            }
            return bestNode;
        }

        public void BackPropagation(Node currentNode, float score)
        {
            for (int i = 0; i < maxNodePathLenght; i++)
            {
                currentNode.value += score;
                if (currentNode.parent == null)
                {
                    break;
                }
                currentNode = currentNode.parent;
            }
        }

        private void GenerateLnArray(int size)
        {
            lnArray = new float[size+1];
            for (int i = 1; i <= size; i++)
            {
                lnArray[i] = Mathf.Log(i);
            }
        }
    }
}