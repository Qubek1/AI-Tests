using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Othello
{
    public class HeuristicsDisplay : MonoBehaviour
    {
        public OthelloHeuristics heuristics;
        public UIGameController UIcontroller;

        public void UpdateVisuals()
        {
            for (int x=0; x<UIcontroller.pawns.GetLength(0); x++)
            {
                for (int y = 0; y< UIcontroller.pawns.GetLength(1); y++)
                {
                    UIcontroller.buttons[x,y].GetComponent<FieldController>().text.text = (Mathf.Round (heuristics.fieldValueTable[0,x,y] * 100) / 100f).ToString();
                }
            }
        }
    }
}