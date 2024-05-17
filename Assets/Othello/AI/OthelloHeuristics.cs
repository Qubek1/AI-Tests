using CI.QuickSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Othello
{
    public class OthelloHeuristics
    {
        public float[,,] fieldValueTable;

        public OthelloHeuristics(float[,,] fieldValueTable)
        {
            this.fieldValueTable = fieldValueTable;
        }

        public float Calculate(int team, OthelloGameModel gameModel)
        {
            float value = 0.0f;
            for (int x = 0; x < gameModel.boardLength; x++)
            {
                for (int y = 0; y < gameModel.boardHeight; y++)
                {
                    if (gameModel.boardState[x, y] == 0)
                        continue;
                    float mult = gameModel.boardState[x, y] == team ? 1f : -1f;
                    value += mult * fieldValueTable[0, x, y];
                }
            }
            return value;
        }

        public void Save(string fileName)
        {
            QuickSaveWriter writer = QuickSaveWriter.Create("Othello Heuristics");
            writer.Write(fileName, fieldValueTable);
            writer.Commit();
        }

        public void Load(string fileName)
        {
            QuickSaveReader reader = QuickSaveReader.Create("Othello Heuristics");
            fieldValueTable = reader.Read<float[,,]>(fileName);
        }
    }
}